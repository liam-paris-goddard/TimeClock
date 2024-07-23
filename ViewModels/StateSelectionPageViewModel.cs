using Goddard.Clock.Controls;
using Goddard.Clock.Models;

namespace Goddard.Clock.ViewModels;
public class StateSelectionPageViewModel : BaseViewModel
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

            States = _schools
                .Select(s => s.State)
                .Distinct()
                .Select(s => new PagedGoddardButtonGridItem
                {
                    Text = s,
                    Value = s,
                })
                .OrderBy(s => s.Text)
                .ToList();
        }
    }

    private List<PagedGoddardButtonGridItem> _states;
    public List<PagedGoddardButtonGridItem> States
    {
        get { return _states; }
        set
        {
            _states = value;
            OnPropertyChanged();
        }
    }

    public IList<AllowedSchool> GetSchoolsByState(string state)
    {
        return Schools.Where(s => string.Equals(s?.State?.ToLower(), state.ToLower())).ToList();
    }

    public StateSelectionPageViewModel()
    {
        _schools = [];
        _states = [];
    }
}
