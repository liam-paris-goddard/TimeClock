using Goddard.Clock.Controls;
using Goddard.Clock.Helpers;
using Goddard.Clock.Data;

namespace Goddard.Clock.ViewModels;
public class EmployeeSelectionPageViewModel : BaseViewModel
{
    private readonly ClockDatabase _database;
    private List<PagedGoddardButtonGridItem>? _gridItems;
    public List<PagedGoddardButtonGridItem> GridItems
    {
        get { return _gridItems!; }
        set
        {
            _gridItems = value;
            OnPropertyChanged();
        }
    }

    public EmployeeSelectionPageViewModel()
    {
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await _database.GetEmployeeList(0, int.MaxValue).ContinueWith((employees) =>
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
                await Logging.Log(_database, ex);
                throw;
            }
        });
    }
}
