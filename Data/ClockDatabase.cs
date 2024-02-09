using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using TimeClock.Models;
using TimeClock.Helpers;
using TimeClock.Data;

namespace TimeClock.Data
{
    public class ClockDatabase
    {
        private readonly SQLiteAsyncConnection database;
        private static AsyncLock _databaseLock = new AsyncLock();

        public ClockDatabase(string dbPath)
        {
            using (_databaseLock.Lock())
            {
                database = new(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                CreateAllTables();
            }
        }

        private void CreateAllTables()
        {
            database.CreateTablesAsync<Event, Parent, Child, Employee, UpdatePIN>().Wait();
            database.CreateTablesAsync<LocalSyncLog, LocalLog>().Wait();
        }
        private void DropAllTables()
        {
            database.DropTableAsync<Event>().Wait();
            database.DropTableAsync<Parent>().Wait();
            database.DropTableAsync<Child>().Wait();
            database.DropTableAsync<Employee>().Wait();
            database.DropTableAsync<UpdatePIN>().Wait();
            database.DropTableAsync<LocalSyncLog>().Wait();
            database.DropTableAsync<LocalLog>().Wait();
        }

        public void RebuildDatatabase()
        {
            using (_databaseLock.Lock())
            {
                DropAllTables();
                CreateAllTables();
            }
        }

        private Task<int> SaveLocalEntityAsync<T>(T item) where T : LocalEntity
        {
            if (item.ID != 0)
            {
                item.Inserted = DateTime.Now;
                item.Updated = DateTime.Now;
                return database.UpdateAsync(item);
            }
            else
            {
                item.Updated = DateTime.Now;
                return database.InsertAsync(item);
            }
        }

        public async Task<bool> SyncRemoteData(PullSyncData data)
        {
            if (data is not { Employees: not null, Parents: not null, Children: not null })
                return false;

            using (await _databaseLock.LockAsync())
            {
                try
                {
                    await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                    {
                        var deleted = transaction.Execute("DELETE from Employee");
                        var inserted = transaction.InsertAll(data.Employees);
                        if (inserted != data.Employees.Length)
                            throw new Exception("Employee rows inserted does not match input data");

                        deleted = transaction.Execute("DELETE from Parent");
                        inserted = transaction.InsertAll(data.Parents);
                        if (inserted != data.Parents.Length)
                            throw new Exception("Parent rows inserted does not match input data");

                        deleted = transaction.Execute("DELETE from Child");
                        inserted = transaction.InsertAll(data.Children);
                        if (inserted != data.Children.Length)
                            throw new Exception("Child rows inserted does not match input data");

                        transaction.Commit();
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(ex);
                    return false;
                }
            }
        }

        public async Task<List<UpdatePIN>> GetUnprocessedUpdatePINs(int rows)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.QueryAsync<UpdatePIN>("select * from UpdatePIN where Uploaded is NULL order by Inserted LIMIT ?", rows);
            }
        }

        public async Task<List<Event>> GetUnprocessedClockEvents(int rows)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.QueryAsync<Event>("select * from Event where Uploaded is NULL order by Inserted LIMIT ?", rows);
            }
        }

        public async Task<List<LocalLog>> GetUnprocessedLogs(int rows)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.QueryAsync<LocalLog>("select * from LocalLog where Uploaded is NULL order by Inserted LIMIT ?", rows);
            }
        }

        public async Task<List<EventExtended>> GetUnprocessedClockExtendedEvents(int rows)
        {
            using (await _databaseLock.LockAsync())
            {
                var events = await database.QueryAsync<Event>("select * from Event where Uploaded is NULL order by Inserted LIMIT ?", rows);
                var children = (await database.Table<Child>().ToListAsync()).ToList<IPerson>();
                var parents = (await database.Table<Parent>().ToListAsync()).ToList<IPerson>();
                var employees = (await database.Table<Employee>().ToListAsync()).ToList<IPerson>();

                var results = new List<EventExtended>();
                foreach (var ev in events)
                {
                    results.Add(new()
                    {
                        ID = ev.ID,
                        UserType = ev.UserType,
                        TargetPersonID = ev.TargetPersonID,
                        UserPersonID = ev.UserPersonID,
                        Occurred = ev.Occurred,
                        Type = ev.Type,
                        UserPersonName = ev.UserType == UserType.Parent ? _GetName(ev.UserPersonID, parents) : _GetName(ev.UserPersonID, employees),
                        TargetPersonName = ev.UserType == UserType.Parent || ev.UserType == UserType.InLocoParentis ? _GetName(ev.TargetPersonID, children) : _GetName(ev.TargetPersonID, employees)
                    });
                }
                return results;
            }
        }
        private string _GetName(long id, IList<IPerson> myList)
        {
            if (myList.Any(@p => @p.PersonID == id))
                return $"{myList.First(@p => @p.PersonID == id).LN}, {myList.First(@p => @p.PersonID == id).FN}";
            else
                return $"Unknown: {id}";
        }

        public async Task<int> PurgeUpdatePIN(int olderThanDays)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.ExecuteAsync("DELETE from UpdatePIN where Uploaded < ?", DateTime.Today.AddDays(-1 * olderThanDays).Ticks);
            }
        }

        public async Task<int> PurgeClockEvent(int olderThanDays)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.ExecuteAsync("DELETE from Event where Uploaded < ?", DateTime.Today.AddDays(-1 * olderThanDays).Ticks);
            }
        }

        public async Task<int> PurgeLocalSyncLog()
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.ExecuteAsync("DELETE FROM LocalSyncLog WHERE ID IN (SELECT ID FROM LocalSyncLog ORDER BY ROWID DESC LIMIT -1 OFFSET 100)");
            }
        }

        public async Task<int> PurgeLocalLog(int olderThanDays)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.ExecuteAsync("DELETE from LocalLog where Uploaded < ?", DateTime.Today.AddDays(-1 * olderThanDays).Ticks);
            }
        }

        public async Task<int> CompactDatabase()
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.ExecuteAsync("vacuum");
            }
        }

        public async Task<List<Employee>> GetEmployeeList(int currentPage, int numberRows)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.Table<Employee>().OrderBy(@e => @e.LN).ThenBy(@e => @e.FN)
                    .Skip(currentPage * numberRows).Take(numberRows).ToListAsync();
            }
        }

        public async Task<Employee?> AuthenticateEmployee(long employeeID, string pin)
        {
            using (await _databaseLock.LockAsync())
            {
                return await HandleAuthenticateEmployee(employeeID, pin) ?? null;
            }
        }

        private async Task<Employee?> HandleAuthenticateEmployee(long employeeID, string pin)
        {
            var employee = await database.Table<Employee>().Where(@e => @e.PersonID == employeeID).FirstOrDefaultAsync();
            if (employee is not null and { PIN: var storedPin } and { PIN: not null } && storedPin == pin)
                return employee;
            else
                return null;
        }
        public async Task<bool> CanEmployeeCheckChildInOut(long employeeID)
        {
            using (await _databaseLock.LockAsync())
            {
                return await _CanEmployeeCheckChildInOut(employeeID);
            }
        }

        private async Task<bool> _CanEmployeeCheckChildInOut(long employeeID)
        {
            var employee = await database.Table<Employee>().Where(@e => @e.PersonID == employeeID).FirstOrDefaultAsync();
            return employee is not null and { AllowChildClockInOut: true };
        }

        public async Task<List<Parent>?> AuthenticateParent(string namestart, string pin)
        {
            using (await _databaseLock.LockAsync())
            {
                if (string.IsNullOrWhiteSpace(namestart) || namestart.Length < 2)
                    return null;

                return namestart.Length >= 3
                    ? await database.Table<Parent>().Where(@p => @p.CN == namestart && @p.PIN == pin && pin != null).ToListAsync()
                    : await database.Table<Parent>().Where(@p => @p.LN.ToUpper() == namestart.ToUpper() && @p.PIN == pin && pin != null).ToListAsync();
            }
        }

        public async Task<Parent?> AuthenticateParent(long parentID, string pin)
        {
            using (await _databaseLock.LockAsync())
            {
                return await HandleAuthenticateParent(parentID, pin);
            }
        }

        private async Task<Parent?> HandleAuthenticateParent(long parentID, string pin)
        {
            var parent = await database.Table<Parent>().Where(@p => @p.PersonID == parentID).FirstOrDefaultAsync();
            return parent is not null and { PIN: var storedPin } and { PIN: not null } && storedPin == pin ? parent : null;
        }

        public async Task<List<Child>> GetChildrenForParent(long parentID)
        {
            using (await _databaseLock.LockAsync())
            {
                return await database.Table<Child>().Where(@c => @c.PID == parentID).ToListAsync();
            }
        }
        public async Task<bool> UpdateEmployeePIN(long employeeID, string oldPIN, string newPIN)
        {
            if (string.IsNullOrWhiteSpace(oldPIN) || string.IsNullOrWhiteSpace(newPIN))
                throw new Exception("Both current and new PIN values are required");

            using (await _databaseLock.LockAsync())
            {
                var employee = await HandleAuthenticateEmployee(employeeID, oldPIN);
                if (employee is not null)
                {
                    employee.PIN = newPIN;
                    employee.ForceResetPIN = null;
                    employee.Updated = DateTime.Now;
                    var updatePIN = new UpdatePIN
                    {
                        UserID = employee.PersonID.HasValue ? (long)employee.PersonID.Value : 0,
                        UserType = UserType.Employee,
                        Action = Models.Action.Change,
                        Old = oldPIN,
                        New = newPIN,
                        Locked = false,
                        Uploaded = null
                    };

                    try
                    {
                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Update(employee);
                            transaction.Insert(updatePIN);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
                else
                {
                    return false; // Old pin did not match
                }
            }
        }

        public async Task<bool> LockEmployeePIN(long employeeID)
        {
            using (await _databaseLock.LockAsync())
            {
                var employee = await database.Table<Employee>().Where(@e => @e.PersonID == employeeID).FirstOrDefaultAsync();
                if (employee is not null)
                {
                    employee.LockedPIN = true;
                    employee.Updated = DateTime.Now;
                    var updatePIN = new UpdatePIN
                    {
                        UserID = (long)employee.PersonID,
                        UserType = UserType.Employee,
                        Action = Models.Action.Lock,
                        Old = employee.PIN,
                        New = employee.PIN,
                        Locked = true,
                        Uploaded = null
                    };

                    try
                    {
                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Update(employee);
                            transaction.Insert(updatePIN);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
                else
                {
                    return false; // Invalid employee ID, most likely can't ever happen unless programmer has messed up
                }
            }
        }

        public async Task<bool> CheckUniquenessPIN(long parentID, string namestart, string newPIN)
        {
            using (await _databaseLock.LockAsync())
            {
                return await _CheckUniquenessPIN(parentID, namestart, newPIN);
            }
        }

        // Will return false if PIN is used by anyone other than the parentID passed
        private async Task<bool> _CheckUniquenessPIN(long parentID, string namestart, string newPIN)
        {
            return await database.Table<Parent>().Where(@p => @p.PersonID != parentID && @p.PIN == newPIN && @p.CN == namestart).CountAsync() == 0;
        }

        public async Task<bool> UpdateParentPIN(long parentID, string namestart, string oldPIN, string newPIN)
        {
            if (string.IsNullOrWhiteSpace(oldPIN) || string.IsNullOrWhiteSpace(newPIN))
                throw new Exception("Both current and new PIN values are required");

            using (await _databaseLock.LockAsync())
            {
                var parent = await HandleAuthenticateParent(parentID, oldPIN);
                if (parent is not null)
                {
                    if (!(await _CheckUniquenessPIN(parentID, namestart, newPIN)))
                        return false;

                    parent.PIN = newPIN;
                    parent.ResetPIN = null;
                    parent.Updated = DateTime.Now;
                    var updatePIN = new UpdatePIN
                    {
                        UserID = (long)parent.PersonID,
                        UserType = UserType.Parent,
                        Action = Models.Action.Change,
                        Old = oldPIN,
                        New = newPIN,
                        Locked = false,
                        Uploaded = null
                    };

                    try
                    {
                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Update(parent);
                            transaction.Insert(updatePIN);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
                else
                {
                    return false; // Old pin did not match
                }
            }
        }

        public async Task<bool> EnterClockEvents(IEnumerable<Event> events)
        {
            if (events is null || !events.Any())
                return false;

            using (await _databaseLock.LockAsync())
            {
                foreach (var ev in events)
                {
                    if (ev.Signature is null or { Length: <= 0 })
                    {
                        if (ev.UserType == UserType.Employee && !Helpers.Settings.BypassSignatureEmployees)
                            return false;
                        else if (ev.UserType == UserType.Parent && !Helpers.Settings.BypassSignatureParents)
                            return false;
                        else if (ev.UserType == UserType.InLocoParentis && !Helpers.Settings.BypassSignatureParents)
                            return false;
                    }

                    if (ev.UserType == UserType.Employee || ev.UserType == UserType.InLocoParentis)
                    {
                        var employee = await database.Table<Employee>().Where(@e => @e.PersonID == ev.UserPersonID).FirstOrDefaultAsync();
                        if (employee is null)
                            throw new Exception("Employee ID invalid");
                    }
                    else if (ev.UserType == UserType.Parent)
                    {
                        var parent = await database.Table<Parent>().Where(@e => @e.PersonID == ev.UserPersonID).FirstOrDefaultAsync();
                        if (parent is null)
                            throw new Exception("Parent ID invalid");
                    }
                    else
                    {
                        throw new Exception($"Invalid User Type: {ev.UserType}");
                    }

                    if (ev.Occurred < DateTime.Now.AddDays(-1) || ev.Occurred > DateTime.Now)
                        throw new Exception("Invalid Clock In/Out Date/Time");
                }

                try
                {
                    await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                    {
                        foreach (var ev in events)
                        {
                            transaction.Insert(ev);
                        }
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(ex);
                    return false;
                }
            }
        }

        public async Task<bool> SetLocalEntitiesUploaded(IEnumerable<LocalEntity> items, DateTime uploaded)
        {
            if (items is null || !items.Any())
                return true; //effectively making this a non-op

            try
            {
                using (await _databaseLock.LockAsync())
                {
                    await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                    {
                        foreach (var item in items)
                        {
                            item.Updated = DateTime.Now;
                            item.Uploaded = uploaded;
                            transaction.Update(item);
                        }
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                await Logging.Log(ex);
                return false;
            }
        }

        public async Task<bool> RegisterSend()
        {
            using (await _databaseLock.LockAsync())
            {
                var log = new LocalSyncLog() { Type = LogType.Send, Occurred = DateTime.Now };
                await SaveLocalEntityAsync(log);
                return true;
            }
        }

        public async Task<bool> RegisterPull()
        {
            using (await _databaseLock.LockAsync())
            {
                var log = new LocalSyncLog() { Type = LogType.Pull, Occurred = DateTime.Now };
                await SaveLocalEntityAsync(log);
                return true;
            }
        }

        public async Task<DateTime?> GetLocalSyncLogLatest(LogType type)
        {
            using (await _databaseLock.LockAsync())
            {
                var value = await database.ExecuteScalarAsync<DateTime>("SELECT MAX(Occurred) FROM LocalSyncLog WHERE Type = ?", type);
                if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
                    return null;
                else
                    return Convert.ToDateTime(value);
            }
        }

        public async Task<List<Child>?> GetChildList(string namestart)
        {
            using (await _databaseLock.LockAsync())
            {
                if (string.IsNullOrWhiteSpace(namestart) || namestart.Length != 1)
                    return null;

                var all = await database.Table<Child>().Where(@p => @p.LN.StartsWith(namestart)).ToListAsync();

                var children = new List<Child>();
                foreach (var child in all.OrderBy(@c => @c.Fullname))
                {
                    if (!children.Any(@c => @c.PersonID == child.PersonID))
                        children.Add(child);
                }

                return children;
            }
        }

        public async Task<List<Child>> GetChildrenForFamily(long familyID)
        {
            using (await _databaseLock.LockAsync())
            {
                var all = await database.Table<Child>().Where(@p => @p.FamilyID == familyID).ToListAsync();

                var children = new List<Child>();
                foreach (var child in all.OrderBy(@c => @c.Fullname))
                {
                    if (!children.Any(@c => @c.PersonID == child.PersonID))
                        children.Add(child);
                }

                return children;
            }
        }

        public async Task<bool> UpdateParent(Parent updated)
        {
            if (updated is null)
                throw new();

            using (await _databaseLock.LockAsync())
            {
                var original = await database.Table<Parent>().Where(@p => @p.PersonID == updated.PersonID).FirstOrDefaultAsync();
                if (original is not null)
                {
                    try
                    {
                        original.Updated = DateTime.Now;
                        original.FN = updated.FN;
                        original.LN = updated.LN;
                        original.PIN = updated.PIN;
                        original.ResetPIN = updated.ResetPIN;
                        original.LockedPIN = updated.LockedPIN;
                        original.PersonID = updated.PersonID;
                        original.FamilyID = updated.FamilyID;

                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Update(original);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        //original not found in local DB, so just do an insert
                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Insert(updated);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
            }
        }

        public async Task<bool> UpdateEmployee(Employee updated)
        {
            if (updated is null)
                throw new();

            using (await _databaseLock.LockAsync())
            {
                var original = await database.Table<Employee>().Where(@p => @p.PersonID == updated.PersonID).FirstOrDefaultAsync();
                if (original is not null)
                {
                    try
                    {
                        original.Updated = DateTime.Now;
                        original.FN = updated.FN;
                        original.LN = updated.LN;
                        original.PIN = updated.PIN;
                        original.ForceResetPIN = updated.ForceResetPIN;
                        original.AllowChildClockInOut = updated.AllowChildClockInOut;
                        original.LockedPIN = updated.LockedPIN;
                        original.PersonID = updated.PersonID;

                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Update(original);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        //original not found in local DB, so just do an insert
                        await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                        {
                            transaction.Insert(updated);
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(ex);
                        return false;
                    }
                }
            }
        }

        public async Task<bool> LocalLogInsert(LocalLog logEntry)
        {
            if (logEntry is null)
                throw new();

            using (await _databaseLock.LockAsync())
            {
                try
                {
                    //original not found in local DB, so just do an insert
                    await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                    {
                        transaction.Insert(logEntry);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.DebugWrite(ex);
                    //TODO: commenting this out as this could lead to infinite recursion, instead we'll have to live with Debug.WriteLine
                    //Logging.Log(ex);
                    return false;
                }
            }
        }
    }
}


/** 

break this file out into smaller classes

also consider changing some of the models into records

*/