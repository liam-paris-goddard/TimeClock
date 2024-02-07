using TimeClock.Controls;
using TimeClock.Helpers;
using TimeClock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public class EmployeeSelectionPageViewModel : BaseViewModel
    {
        private List<PagedGoddardButtonGridItem> _gridItems;
        public List<PagedGoddardButtonGridItem> GridItems
        {
            get { return _gridItems; }
            set
            {
                _gridItems = value;
                OnPropertyChanged();
            }
        }

        public EmployeeSelectionPageViewModel()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    App.Database.GetEmployeeList(0, int.MaxValue).ContinueWith((employees) =>
                        {
                            GridItems = employees.Result
                                .Select(s => new PagedGoddardButtonGridItem
                                {
                                    Text = s.Fullname,
                                    Value = s.PersonID.ToString()
                                })
                                .OrderBy(s => s.Text)
                                .ToList();
                        });
                }
                catch (Exception ex)
                {
                    TimeClock.Helpers.Logging.Log(ex);
                    throw;
                }
            });
        }
    }
}
