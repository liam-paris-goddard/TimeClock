using TimeClock.Controls;
using TimeClock.Helpers;
using TimeClock.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public class ParentSelectionPageViewModel : BaseViewModel
    {
        private string _PIN;

        public string PIN
        {
            get { return _PIN; }
            set
            {
                _PIN = value;
                OnPropertyChanged();
            }
        }

        private List<Parent> _parents;
        public List<Parent> Parents
        {
            get { return _parents; }
            set
            {
                _parents = value;
                OnPropertyChanged();

                if (_parents == null)
                    return;

                GridItems = _parents
                    .Select(p => new PagedGoddardButtonGridItem
                    {
                        Text = p.Fullname,
                        Value = p.PersonID.ToString(),
                    })
                    .OrderBy(p => p.Text)
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
