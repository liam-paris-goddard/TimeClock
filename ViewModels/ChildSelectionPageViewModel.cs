using Goddard.Clock.Controls;
using Goddard.Clock.Models;


namespace Goddard.Clock.ViewModels;
public class ChildSelectionPageViewModel : BaseViewModel
{
    private long? _employeeUserPersonId;
    public long? EmployeeUserPersonId
    {
        get { return _employeeUserPersonId; }
        set
        {
            _employeeUserPersonId = value;
            OnPropertyChanged();
        }
    }

    private string? _employeeUserFN;
    public string EmployeeUserFN
    {
        get { return _employeeUserFN!; }
        set
        {
            _employeeUserFN = value;
            OnPropertyChanged();
        }
    }

    private string? _employeeUserLN;
    public string EmployeeUserLN
    {
        get { return _employeeUserLN!; }
        set
        {
            _employeeUserLN = value;
            OnPropertyChanged();
        }
    }

    private List<Child>? _children;
    public List<Child> Children
    {
        get { return _children!; }
        set
        {
            _children = value;
            OnPropertyChanged();

            if (_children == null)
                return;

            GridItems = _children
                .Select(p => new PagedGoddardButtonGridItem
                {
                    Text = p.Fullname,
                    Value = p.FamilyID.ToString(),
                })
                .OrderBy(p => p.Text)
                .ToList();
        }
    }

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
}
