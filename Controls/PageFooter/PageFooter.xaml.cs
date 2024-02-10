using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    public partial class PageFooter : ContentView
    {
        public event EventHandler LeftButtonClickHandler;
        public event EventHandler CenterButtonClickHandler;
        public event EventHandler RightButtonClickHandler;


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
            get => _leftButtonText;
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
            get => _centerButtonText;
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
            get => _rightButtonText;
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


        /*#if DEBUG
                schoolLabel.Text = Helpers.Settings.LastSelectedSchoolName + " (T)";
                #else
                    schoolLabel.Text = Helpers.Settings.LastSelectedSchoolName;
                #endif  TODO - figure out */

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(ShowLogo):
                    logoImage.IsVisible = ShowLogo;
                    break;
                case nameof(ShowLeftButton):
                    leftButton.IsVisible = ShowLeftButton;
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

        protected async void LeftButtonClick(object sender, EventArgs e)
        {
            if (LeftButtonAutoGoBack)
                try { await Navigation.PopAsync(false); }
                catch { }
            else if (LeftButtonAutoGoToMain)
                await Helpers.Navigation.ResetNavigationAndGoToRoot();
            else
                LeftButtonClickHandler?.Invoke(this, e);
        }

        protected async void CenterButtonClick(object sender, EventArgs e)
        {
            if (CenterButtonIsExitEmpMode)
            {
                App.EmployeeUserPersonID = null;
                await Helpers.Navigation.ResetNavigationAndGoToRoot();
            }
            else
            {
                CenterButtonClickHandler?.Invoke(this, e);
            }
        }

        protected void RightButtonClick(object sender, EventArgs e)
        {
            RightButtonClickHandler?.Invoke(this, e);
        }
    }
}

/*
 TODO - consider
 please note that the Navigation class in .NET MAUI is slightly different from Xamarin.Forms. You might need to adjust the navigation-related code to fit the new API.

*/