using System.Runtime.CompilerServices;
using Goddard.Clock.Models;

namespace Goddard.Clock.Controls;
public partial class CheckInSelector : BaseContentView
{
    public event EventHandler? SelectionMade;

    public enum CheckInSelectorImageType
    {
        Employee,
        Boy,
        Girl
    }

    private string? _firstName;
    public string FirstName
    {
        get => _firstName!;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    private string? _lastName;
    public string LastName
    {
        get => _lastName!;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }

    private long _personId;
    public long PersonId
    {
        get => _personId;
        set
        {
            _personId = value;
            OnPropertyChanged();
        }
    }

    private string? _classroom;
    public string Classroom
    {
        get => _classroom!;
        set
        {
            _classroom = value;
            OnPropertyChanged();
        }
    }

    private CheckInSelectorImageType _imageType;
    public CheckInSelectorImageType ImageType
    {
        get => _imageType;
        set
        {
            _imageType = value;
            OnPropertyChanged();
        }
    }

    public bool IsActive
    {
        get => PersonId > 0;
        private set { }
    }


    private ClockEventType? _selectedEventType;
    public ClockEventType? SelectedEventType { 
        get => _selectedEventType; 
        set 
        {
            _selectedEventType = value;
            OnPropertyChanged();
        
        } 
    }

    public string FullName
    {
        get => $"{LastName}, {FirstName}";
        internal set { }
    }

    public CheckInSelector()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        try
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                case nameof(LastName):
                    nameLabel.Text = $"{FirstName}\n{LastName}";
                    break;
                case nameof(Classroom):
                    classroomLabel.Text = Classroom;
                    break;
                case nameof(ImageType):
                    switch (ImageType)
                    {
                        case CheckInSelectorImageType.Employee:
                            personImage.Source = "employee.png";
                            break;
                        case CheckInSelectorImageType.Boy:
                            personImage.Source = "boy.png";
                            break;
                        case CheckInSelectorImageType.Girl:
                            personImage.Source = "girl.png";
                            break;
                    }
                    break;
                case nameof(SelectedEventType):
                    UpdateButtonColors();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void checkInButton_Clicked(object? sender, EventArgs e)
    {
        if (SelectedEventType == ClockEventType.In)
        {
            SelectedEventType = null;
        }
        else
        {
            SelectedEventType = ClockEventType.In;
            if (this.SelectionMade != null)
                this.SelectionMade(this, e);
        }

    }

    private void checkOutButton_Clicked(object? sender, EventArgs e)
    {
        if (SelectedEventType == ClockEventType.Out)
        {
            SelectedEventType = null;
        }
        else
        {
            SelectedEventType = ClockEventType.Out;
            if (this.SelectionMade != null)
                this.SelectionMade(this, e);
        }

    }

    private void UpdateButtonColors()
    {
        checkInButton.PersistAltColor = SelectedEventType == ClockEventType.In;
        checkOutButton.PersistAltColor = SelectedEventType == ClockEventType.Out;
    }

    public void ChangeSelectedEventType(ClockEventType eventType)
    {
        var temp = EventArgs.Empty;
        if (SelectedEventType != eventType)
        {
            if (eventType == ClockEventType.In)
                checkInButton_Clicked(null, temp);
            else if (eventType == ClockEventType.Out)
                checkOutButton_Clicked(null, temp);
        }
    }
}
