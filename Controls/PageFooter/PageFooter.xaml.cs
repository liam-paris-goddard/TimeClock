using System.Runtime.CompilerServices;
using Goddard.Clock.Helpers;

namespace Goddard.Clock.Controls;
public partial class PageFooter : BaseContentView
{
    public event EventHandler? LeftButtonClickHandler;
    public event EventHandler? CenterButtonClickHandler;
    public event EventHandler? RightButtonClickHandler;

    private double _buttonFontSize = 32; //20 when smaller

    public double ButtonFontSize
    {
        get => _buttonFontSize;
        set
        {
            _buttonFontSize = value;
            OnPropertyChanged();
        }
    }

    [Obsolete]
    private LayoutOptions _centerHorizontalOption = LayoutOptions.CenterAndExpand;

    [Obsolete]
    public LayoutOptions CenterHorizontalOption
    {
        get => _centerHorizontalOption;
        set
        {
            _centerHorizontalOption = value;
            OnPropertyChanged();
        }
    }

    private double _buttonHeightSize = 75; //50 when smaller
    public double ButtonHeightSize
    {
        get => _buttonHeightSize;
        set
        {
            _buttonHeightSize = value;
            OnPropertyChanged();
        }
    }
    private double _logoImageWidth = 340;
    public double LogoImageWidth
    {
        get => _logoImageWidth;
        private set
        {
            _logoImageWidth = value;
            OnPropertyChanged();
        }
    }

    private double _buttonWidthSize = 170; //100 when smaller
    public double ButtonWidthSize
    {
        get => _buttonWidthSize;
        set
        {
            _buttonWidthSize = value;
            LogoImageWidth = 2 * _buttonWidthSize; // Update LogoImageWidth whenever ButtonWidthSize is updated
            OnPropertyChanged();
        }
    }
    private Thickness _leftButtonMargin = new Thickness(60, 0, 0, 0); //30,0,0,0 when smaller
    public Thickness LeftButtonMargin
    {
        get => _leftButtonMargin;
        set
        {
            _leftButtonMargin = value;
            OnPropertyChanged();
        }
    }
    private Thickness _rightButtonMargin = new Thickness(0, 0, 60, 0); //0,0,30,0 when smaller
    public Thickness RightButtonMargin
    {
        get => _rightButtonMargin;
        set
        {
            _rightButtonMargin = value;
            OnPropertyChanged();
        }
    }

    private readonly NavigationService? _navigation;

    [Obsolete]
    public PageFooter()
    {
        SetResponsiveVars();
        BindingContext = this;
        InitializeComponent();
        CenterHorizontalOption = ShowLeftButton ? LayoutOptions.CenterAndExpand : LayoutOptions.StartAndExpand; _navigation = App.NavigationService;

#if DEBUG
        schoolLabel.Text = Settings.LastSelectedSchoolName + " (T)";
#else
        schoolLabel.Text = Helpers.Settings.LastSelectedSchoolName;
#endif
    }


    private bool _showLogo = true;
    public bool ShowLogo
    {
        get => _showLogo;
        set
        {
            _showLogo = value;
            OnPropertyChanged();
        }
    }

    private bool _showLeftButton = true;
    public bool ShowLeftButton
    {
        get => _showLeftButton;
        set
        {
            _showLeftButton = value;
            OnPropertyChanged();
        }
    }

    private string? _leftButtonText;
    public string LeftButtonText
    {
        get => _leftButtonText!;
        set
        {
            _leftButtonText = value;
            OnPropertyChanged();
        }
    }

    private bool _leftButtonAutoGoBack;
    public bool LeftButtonAutoGoBack
    {
        get => _leftButtonAutoGoBack;
        set
        {
            _leftButtonAutoGoBack = value;
            OnPropertyChanged();
        }
    }

    private bool _leftButtonAutoGoToMain;
    public bool LeftButtonAutoGoToMain
    {
        get => _leftButtonAutoGoToMain;
        set
        {
            _leftButtonAutoGoToMain = value;
            OnPropertyChanged();
        }
    }

    private bool _showCenterButton = true;
    public bool ShowCenterButton
    {
        get => _showCenterButton;
        set
        {
            _showCenterButton = value;
            OnPropertyChanged();
        }
    }

    private string? _centerButtonText;
    public string CenterButtonText
    {
        get => _centerButtonText!;
        set
        {
            _centerButtonText = value;
            OnPropertyChanged();
        }
    }

    private bool _centerButtonUseAltColor = false;
    public bool CenterButtonUseAltColor
    {
        get => _centerButtonUseAltColor;
        set
        {
            _centerButtonUseAltColor = value;
            OnPropertyChanged();
        }
    }

    private bool _centerButtonIsExitEmpMode;

    public bool CenterButtonIsExitEmpMode
    {
        get => _centerButtonIsExitEmpMode;
        set
        {
            _centerButtonIsExitEmpMode = value;
            OnPropertyChanged();
        }
    }

    private bool _showRightButton = true;
    public bool ShowRightButton
    {
        get => _showRightButton;
        set
        {
            _showRightButton = value;
            OnPropertyChanged();
        }
    }

    private string? _rightButtonText;
    public string RightButtonText
    {
        get => _rightButtonText!;
        set
        {
            _rightButtonText = value;
            OnPropertyChanged();
        }
    }

    private bool _showSchoolLabel;
    public bool ShowSchoolLabel
    {
        get => _showSchoolLabel;
        set
        {
            _showSchoolLabel = value;
            OnPropertyChanged();
        }
    }

    private string _centerText = "";
    public string CenterText
    {
        get => _centerText;
        set
        {
            _centerText = value;
            OnPropertyChanged();
        }
    }


    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(ShowLogo):
                logoImage.IsVisible = ShowLogo;
                break;
            case nameof(ShowLeftButton):
                leftButton.IsVisible = ShowLeftButton;
                CenterHorizontalOption = ShowLeftButton ? LayoutOptions.CenterAndExpand : LayoutOptions.StartAndExpand;
                break;
            case nameof(LeftButtonText):
                leftButton.Text = LeftButtonText;
                break;
            case nameof(ShowCenterButton):
                centerButton.IsVisible = ShowCenterButton;
                if (ShowCenterButton)
                    logoImage.IsVisible = false;
                break;
            case nameof(CenterButtonText):
                centerButton.Text = CenterButtonText;
                break;
            case nameof(CenterButtonUseAltColor):
                centerButton.UseAltColor = CenterButtonUseAltColor;
                break;
            case nameof(CenterButtonIsExitEmpMode):
                centerButton.UseAltColor = true;
                centerButton.Text = "EXIT EMP/CHILD MODE";
                centerButton.FontSize = centerButton.FontSize * 0.6;
                break;
            case nameof(ShowRightButton):
                rightButton.IsVisible = ShowRightButton;
                break;
            case nameof(RightButtonText):
                rightButton.Text = RightButtonText;
                break;
            case nameof(ShowSchoolLabel):
                schoolLabel.IsVisible = ShowSchoolLabel;
                break;
            case nameof(CenterText):
                if (!String.IsNullOrEmpty(CenterText))
                {
                    logoImage.IsVisible = false;
                    centerTextLabel.IsVisible = true;
                    centerTextLabel.Text = CenterText;
                }
                break;
        }
    }

    protected override void OnDeviceInformationChanged(string propertyName)
    {
        if (propertyName == "DeviceWidth" || propertyName == "GlobalOrientation" || propertyName == "DeviceHeight")
        {
            SetResponsiveVars();
        }

    }
    public void SetResponsiveVars()
    {
        ButtonFontSize = 32;
        ButtonHeightSize = 75;
        ButtonWidthSize = 170;
        LeftButtonMargin = new Thickness(60, 0, 0, 0);
        RightButtonMargin = new Thickness(0, 0, 60, 0);

        if (DeviceType == "small")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
                ButtonFontSize = 20;
                ButtonHeightSize = 50;
                ButtonWidthSize = 120;
                LeftButtonMargin = new Thickness(30, 0, 0, 0);
                RightButtonMargin = new Thickness(0, 0, 30, 0);
            }
            else
            {
                ButtonFontSize = 20;
                ButtonHeightSize = 50;
                ButtonWidthSize = 120;
                LeftButtonMargin = new Thickness(30, 0, 0, 0);
                RightButtonMargin = new Thickness(0, 0, 30, 0);
            }
        }

    }


    protected async void LeftButtonClick(object? sender, EventArgs e)
    {
        if (LeftButtonAutoGoBack){    
            _ = await Navigation.PopAsync(false);
        }
        else if (LeftButtonAutoGoToMain && _navigation != null) {
            await _navigation.ResetNavigationAndGoToRoot(); 
        }
        else {
            LeftButtonClickHandler?.Invoke(this, e);
        }
    }

    protected async void CenterButtonClick(object? sender, EventArgs e)
    {
        if (CenterButtonIsExitEmpMode)
        {
            App.EmployeeUserPersonId = null;
            if (App.NavigationService != null)
                await App.NavigationService.ResetNavigationAndGoToRoot();
        }
        else
        {
            CenterButtonClickHandler?.Invoke(this, e);
        }
    }

    protected void RightButtonClick(object? sender, EventArgs e)
    {
        RightButtonClickHandler?.Invoke(this, e);
    }
}
