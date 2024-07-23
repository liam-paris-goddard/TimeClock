using Goddard.Clock.Controls;
using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.Models;
using Goddard.Clock.Factories;
using System.Runtime.CompilerServices;

namespace Goddard.Clock;
public partial class HomePage : UntimedContentPage
{
    private readonly ClockDatabase _database;
    private readonly IServiceProvider _serviceProvider;
    private readonly SyncEngineService _syncEngine;
    public double _welcomeImageWidth = 550;
    public double WelcomeImageWidth
    {
        get { return _welcomeImageWidth; }
        set
        {
            if (_welcomeImageWidth != value)
            {
                _welcomeImageWidth = value;
                OnPropertyChanged();
            }
        }
    }


    public double _stackLayoutSpacing = 20;
    public double StackLayoutSpacing
    {
        get { return _stackLayoutSpacing; }
        set
        {
            if (_stackLayoutSpacing != value)
            {
                _stackLayoutSpacing = value;
                OnPropertyChanged();
            }
        }
    }

    public bool _welcomeImageVisible = true;
    public bool WelcomeImageVisible
    {
        get { return _welcomeImageVisible; }
        set
        {
            if (_welcomeImageVisible != value)
            {
                _welcomeImageVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public void SetResponsiveVars()
    {

        WelcomeImageWidth = 550;
        StackLayoutSpacing = 20;
        if (DeviceType == "small")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
                WelcomeImageWidth = 550;
                StackLayoutSpacing = 20;
            }
            else
            {
                WelcomeImageWidth = 300;
                StackLayoutSpacing = 15;
            }
        }

    }



    public HomePage(IServiceProvider serviceProvider, bool showWelcomeImage = true)    {
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        _serviceProvider = serviceProvider;
        _syncEngine = App.SyncEngine ?? throw new ArgumentNullException(nameof(App.SyncEngine));
        BindingContext = this;
        WelcomeImageVisible = showWelcomeImage;
        SetResponsiveVars();
        InitializeComponent();

        letterButtons.LetterButtonClickHandler += new EventHandler<LetterButtons.LetterButtonClickEventArgs>(LetterButtonClick);

        if (App.EmployeeUserPersonId.HasValue)
        {
            letterButtons.Title = "CHILD'S LAST NAME";
            letterButtons.IsTypingMode = false;
            footer.ShowCenterButton = true;
        }
        else
        {
            footer.ShowCenterButton = false;
            letterButtons.TypingModeSubmitButtonClickHandler += new EventHandler<LetterButtons.TypingModeSubmitButtonClickEventArgs>(LetterButtonsTypingModeSubmitButtonClick);
            letterButtons.Title = "LAST NAME";
            letterButtons.IsTypingMode = true;
            letterButtons.TypingModeMaxLength = 3;
            letterButtons.TypingModeMinLength = 3;
        }


    }

    protected override void OnDeviceInformationChanged(string propertyName)
    {
        if (propertyName == "DeviceWidth" || propertyName == "GlobalOrientation" || propertyName == "DeviceHeight")
        {
            SetResponsiveVars();
        }

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        letterButtons.ClearTypingModeText();

        if (App.EmployeeUserPersonId.HasValue)
        {
            // this is by default an untimed page, but for employee/child mode we still want to go to normal version of main page when inactive.
            GlobalResources.Current.GoToMainOnPageTimeout = true;
        }
        //TODO: not sure if this is best place for this as this page gets instantiated many times
        //but this is also the one place where we know the credentials have been supplied and school
        //selected and will always be seen, since Start on an already Started engine is a non-op, it won't
        //hurt to have it here for sure
        await _syncEngine.Start();
    }

    protected async void LetterButtonClick(object? sender, LetterButtons.LetterButtonClickEventArgs? e)
    {
        if (e != null && e.IsEmployeeButton)
        {
            var factory = _serviceProvider.GetRequiredService<IEmployeeSelectionPageFactory>();
            var page = factory.Create();
            await Navigation.PushAsync(page, false);
            return;
        }

        if (e != null && App.EmployeeUserPersonId.HasValue)
        {
            var children = await _database.GetChildList(e.SelectedLetter);
            var factory = _serviceProvider.GetRequiredService<IChildSelectionPageFactory>();
            var page = factory.Create(children, App.EmployeeUserPersonId);
            await Navigation.PushAsync(page, false);
        }

        // if not employee mode then the LetterButtons control should be in typing mode and normal letter buttons are just used 
        // by the user control itself to enter text and don't need to be handled here.
    }

    protected async void LetterButtonsTypingModeSubmitButtonClick(object? sender, LetterButtons.TypingModeSubmitButtonClickEventArgs? e)
    {
        if (e == null || String.IsNullOrWhiteSpace(e.EnteredText))
            return;

        else if (e.EnteredText.Length < 1)
        {
            await DisplayAlert("", "please enter up to the first three letters of your last name", "OK");
            return;
        }
        var factory = _serviceProvider.GetRequiredService<IPinPadPageFactory>();
        var page = factory.Create(UserType.Parent, e.EnteredText);
        await Navigation.PushAsync(page,
            false);
    }
}
