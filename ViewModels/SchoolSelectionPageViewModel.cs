using TimeClock.Controls;
using TimeClock.Helpers;
using TimeClock.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TimeClock
{
    public class SchoolSelectionPageViewModel : BaseViewModel
    {
        private List<AllowedSchool> _schools;
        public List<AllowedSchool> Schools
        {
            get { return _schools; }
            set
            {
                _schools = value;
                OnPropertyChanged();

                if (_schools == null)
                    return;

                GridItems = _schools
                    .Select(s => new PagedGoddardButtonGridItem
                    {
                        Text = s.Name,
                        Value = s.ID.ToString(),
                    })
                    .OrderBy(s => s.Text)
                    .ToList();
            }
        }

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
    }
}
