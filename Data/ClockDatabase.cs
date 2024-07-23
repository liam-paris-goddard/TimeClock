using SQLite;
using Goddard.Clock.Models;
using Goddard.Clock.Helpers;

namespace Goddard.Clock.Data;
public class ClockDatabase
{
    private readonly SQLiteAsyncConnection database;

    //distinct from similarily named semaphore in the sync engine, this makes it so that only
    //one method is ever operating on the database at one moment
    private static readonly AsyncLock _databaseLock = new();

    public ClockDatabase(string dbPath)
    {
        using (_databaseLock.Lock())
        {
            database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            _CreateAllTables();
        }
    }

    private void _CreateAllTables()
    {
        database.CreateTablesAsync<Event, Parent, Child, Employee, UpdatePIN>().Wait();
        database.CreateTablesAsync<LocalSyncLog, LocalLog>().Wait();
    }

    private void _DropAllTables()
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
            _DropAllTables();
            _CreateAllTables();
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
        if (data == null || data.Employees == null || data.Parents == null || data.Children == null)
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
                await Logging.Log(this, ex);
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
                results.Add(new EventExtended()
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
            return myList.First(@p => @p.PersonID == id).LN + ", " + myList.First(@p => @p.PersonID == id).FN;
        else
            return "Unknown: " + id.ToString();
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
            return await database.Table<Employee>().OrderBy(@e => @e.LN).OrderBy(@e => @e.FN)
                .Skip(currentPage).Take(numberRows).ToListAsync();
        }
    }

    public async Task<Employee> AuthenticateEmployee(long employeeID, string pin)
    {
        using (await _databaseLock.LockAsync())
        {
            return await _AuthenticateEmployee(employeeID, pin);
        }
    }

    private async Task<Employee> _AuthenticateEmployee(long employeeID, string pin)
    {
        var employee = await database.Table<Employee>().Where(@e => @e.PersonID == employeeID).FirstOrDefaultAsync();
        if (employee != null && employee.PIN == pin && pin != null)
            return employee;
        else
            return null!;
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
        return employee != null && employee.AllowChildClockInOut;
    }

    public async Task<List<Parent>> AuthenticateParent(string namestart, string pin)
    {
        using (await _databaseLock.LockAsync())
        {
            if (String.IsNullOrWhiteSpace(namestart))
                return null!;

            return await database.Table<Parent>().Where(@p => @p.CN == namestart && @p.PIN == pin && pin != null).ToListAsync();
            /*else
            {
                return await database.Table<Parent>().Where(@p => @p.LN != null && @p.LN.Equals(namestart, StringComparison.CurrentCultureIgnoreCase) && (@p.PIN == pin && pin != null)).ToListAsync();
            }*/
        }
    }

    public async Task<Parent> AuthenticateParent(long parentID, string pin)
    {
        using (await _databaseLock.LockAsync())
        {
            return await _AuthenticateParent(parentID, pin);
        }
    }

    private async Task<Parent> _AuthenticateParent(long parentID, string pin)
    {
        var parent = await database.Table<Parent>().Where(@p => @p.PersonID == parentID).FirstOrDefaultAsync();
        if (parent != null && parent.PIN == pin && pin != null)
            return parent;
        else
            return null!;
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
        if (String.IsNullOrWhiteSpace(oldPIN) || String.IsNullOrWhiteSpace(newPIN))
            throw new Exception("both current and new PIN values are required");

        using (await _databaseLock.LockAsync())
        {
            var employee = await _AuthenticateEmployee(employeeID, oldPIN);
            if (employee != null)
            {
                employee.PIN = newPIN;
                employee.ForceResetPIN = null;
                employee.Updated = DateTime.Now;
                var updatePIN = new UpdatePIN()
                {
                    UserID = (long)employee.PersonID!,
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
                        _ = transaction.Update(employee);
                        _ = transaction.Insert(updatePIN);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
                    return false;
                }
            }
            else
            {
                return false; //old pin did not match
            }
        }
    }

    public async Task<bool> LockEmployeePIN(long employeeID)
    {
        using (await _databaseLock.LockAsync())
        {
            var employee = await database.Table<Employee>().Where(@e => @e.PersonID == employeeID).FirstOrDefaultAsync();
            if (employee != null)
            {
                employee.LockedPIN = true;
                employee.Updated = DateTime.Now;
                var updatePIN = new UpdatePIN()
                {
                    UserID = (long)employee.PersonID!,
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
                        _ = transaction.Update(employee);
                        _ = transaction.Insert(updatePIN);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
                    return false;
                }
            }
            else
            {
                return false; //invalid employee ID, most likely can't ever happen unless programmer has messed up
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

    //will return false if PIN is used by anyone other than the parentID passed
    private async Task<bool> _CheckUniquenessPIN(long parentID, string namestart, string newPIN)
    {
        return await database.Table<Parent>().Where(@p => @p.PersonID != parentID && @p.PIN == newPIN && @p.CN == namestart).CountAsync() == 0;
    }

    public async Task<bool> UpdateParentPIN(long parentID, string namestart, string oldPIN, string newPIN)
    {
        if (String.IsNullOrWhiteSpace(oldPIN) || String.IsNullOrWhiteSpace(newPIN))
            throw new Exception("both current and new PIN values are required");

        using (await _databaseLock.LockAsync())
        {
            var parent = await _AuthenticateParent(parentID, oldPIN);
            if (parent != null)
            {
                if (!(await _CheckUniquenessPIN(parentID, namestart, newPIN)))
                    return false;

                parent.PIN = newPIN;
                parent.ResetPIN = null;
                parent.Updated = DateTime.Now;
                var updatePIN = new UpdatePIN()
                {
                    UserID = (long)parent.PersonID!,
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
                        _ = transaction.Update(parent);
                        _ = transaction.Insert(updatePIN);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
                    return false;
                }
            }
            else
            {
                return false; //old pin did not match
            }
        }
    }

    public async Task<bool> EnterClockEvents(IEnumerable<Event> events)
    {
        //TODO: add check in here for single target with more than 10 clock ins or 10 clock outs in single
        //day and provide feedback to user
        if (events == null || events.Count() < 1)
            return false;

        using (await _databaseLock.LockAsync())
        {
            foreach (var ev in events)
            {
                if (ev.Signature == null || ev.Signature.Length <= 0)
                {
                    if (ev.UserType == UserType.Employee && !Settings.BypassSignatureEmployees)
                        return false;
                    else if (ev.UserType == UserType.Parent && !Settings.BypassSignatureParents)
                        return false;
                    else if (ev.UserType == UserType.InLocoParentis && !Settings.BypassSignatureParents)
                        return false;
                }

                //validating user ID
                if (ev.UserType == UserType.Employee || ev.UserType == UserType.InLocoParentis)
                {
                    var employee = await database.Table<Employee>().Where(@e => @e.PersonID == ev.UserPersonID).FirstOrDefaultAsync();
                    if (employee == null)
                        throw new Exception("Employee ID invalid");
                }
                else if (ev.UserType == UserType.Parent)
                {
                    var parent = await database.Table<Parent>().Where(@e => @e.PersonID == ev.UserPersonID).FirstOrDefaultAsync();
                    if (parent == null)
                        throw new Exception("Parent ID invalid");
                }
                else
                {
                    //only would happen if developer adds a new user type and forgets to add appropriate handling
                    throw new Exception("Invalid User Type: " + ev.UserType.ToString());
                }

                //sanity check the date
                if (ev.Occurred < DateTime.Now.AddDays(-1)) //why would you be saving a date more than 24 hours in the past, remember this is the live/local method
                    throw new Exception("Invalid Clock In/Out Date/Time");

                if (ev.Occurred > DateTime.Now) //entering a datetime in the future, oh no sir!
                    throw new Exception("Invalid Clock In/Out Date/Time");
            }

            //ok all the input looks reasonable, let's proceed
            try
            {
                await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                {
                    foreach (var ev in events)
                    {
                        _ = transaction.Insert(ev);
                    }
                });
                return true; //if no exception, then transaction passed
            }
            catch (Exception ex)
            {
                await Logging.Log(this, ex);
                return false;
            }
        }
    }

    public async Task<bool> SetLocalEntitiesUploaded(IEnumerable<LocalEntity> items, DateTime uploaded)
    {
        if (items == null || items.Count() < 1)
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
                        _ = transaction.Update(item);
                    }
                });
                return true;
            }
        }
        catch (Exception ex)
        {
            await Logging.Log(this, ex);
            return false;
        }
    }

    public async Task<bool> RegisterSend()
    {
        using (await _databaseLock.LockAsync())
        {
            var log = new LocalSyncLog() { Type = LogType.Send, Occurred = DateTime.Now };
            _ = await SaveLocalEntityAsync<LocalSyncLog>(log);
            return true;
        }
    }

    public async Task<bool> RegisterPull()
    {
        using (await _databaseLock.LockAsync())
        {
            var log = new LocalSyncLog() { Type = LogType.Pull, Occurred = DateTime.Now };
            _ = await SaveLocalEntityAsync<LocalSyncLog>(log);
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

    public async Task<List<Child>> GetChildList(string namestart)
    {
        using (await _databaseLock.LockAsync())
        {
            if (String.IsNullOrWhiteSpace(namestart) || namestart.Length != 1)
                return null!;

            var all = await database.Table<Child>().Where(@p => @p.LN != null && @p.LN.StartsWith(namestart)).ToListAsync();

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
        if (updated == null)
            throw new ArgumentNullException();

        using (await _databaseLock.LockAsync())
        {
            var original = await database.Table<Parent>().Where(@p => @p.PersonID == updated.PersonID).FirstOrDefaultAsync();
            if (original != null)
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
                        _ = transaction.Update(original);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
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
                        _ = transaction.Insert(updated);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
                    return false;
                }
            }
        }
    }



    public async Task<bool> UpdateEmployee(Employee updated)
    {
        if (updated == null)
            throw new ArgumentNullException();

        using (await _databaseLock.LockAsync())
        {
            var original = await database.Table<Employee>().Where(@p => @p.PersonID == updated.PersonID).FirstOrDefaultAsync();
            if (original != null)
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
                        _ = transaction.Update(original);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
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
                        _ = transaction.Insert(original);
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    await Logging.Log(this, ex);
                    return false;
                }
            }
        }
    }




    public async Task<bool> LocalLogInsert(LocalLog logEntry)
    {
        if (logEntry == null)
            throw new ArgumentNullException();

        using (await _databaseLock.LockAsync())
        {
            try
            {
                //original not found in local DB, so just do an insert
                await database.RunInTransactionAsync((SQLiteConnection transaction) =>
                {
                    _ = transaction.Insert(logEntry);
                });
                return true;
            }
            catch (Exception ex)
            {
                Logging.DebugWrite(ex);
                //TODO: commenting this out as this could lead to infinite recursion, instead we'll have to live with Debug.WriteLine
                //await Logging.Log(this,ex);
                return false;
            }
        }
    }

}
