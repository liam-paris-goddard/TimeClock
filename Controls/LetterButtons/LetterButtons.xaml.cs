using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Goddard.Clock.Controls;
using Goddard.Clock.Helpers;
using System.Diagnostics;
using System.ComponentModel;
namespace Goddard.Clock.Controls;
public partial class LetterButtons : BaseContentView
{
    public Label titleLabel = new Label();
    public Label typingModeLabel = new Label();
    public BorderlessEntry typingModeEntry = new BorderlessEntry();
    public GoddardButton typingModeSubmitButton = new GoddardButton();
    public GoddardButton typingModeClearButton = new GoddardButton();

    public int ColumnCount = 9;
    public double _mainGridWidth;

    public double MainGridWidth
    {
        get => _mainGridWidth;
        set
        {
            _mainGridWidth = value;
            OnPropertyChanged();
        }
    }
    public double LetterButtonFontSize;
    public double LetterButtonSize;
    public LetterButtons()
    {
        BindingContext = this;

        InitializeComponent();
        SetResponsiveVars();
        AddToGrid();
    }

    public void SetResponsiveVars()
    {
        LetterButtonSize = 80;
        LetterButtonFontSize = 32;
        //MainGridWidth = 800;

        /*if (DeviceType == "small")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
                LetterButtonSize = 60;
                LetterButtonFontSize = 24;
                MainGridWidth = 500;
            }
            else
            {
                LetterButtonSize = 60;
                LetterButtonFontSize = 24;
                MainGridWidth = 500;
            }
        }*/
        /*else if (DeviceType == "medium")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
                LetterButtonSize = 100;
                LetterButtonFontSize = 32;
                MainGridWidth = 800;
            }
            else
            {
                LetterButtonSize = 60;
                LetterButtonFontSize = 24;
                MainGridWidth = 500;
            }
        }*/
    }

    protected override void OnDeviceInformationChanged(string propertyName)
    {
        if (propertyName == "DeviceWidth" || propertyName == "GlobalOrientation" || propertyName == "DeviceHeight")
        {
            SetResponsiveVars();
            CreateButtons();
        }

    }

    public event EventHandler<LetterButtonClickEventArgs>? LetterButtonClickHandler;
    public event EventHandler<TypingModeSubmitButtonClickEventArgs>? TypingModeSubmitButtonClickHandler;
    private string? _title;
    public string Title
    {
        get => _title!;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    private bool _isTypingMode;
    public bool IsTypingMode
    {
        get => _isTypingMode;
        set
        {
            _isTypingMode = value;
            OnPropertyChanged();
        }
    }

    private int _typingModeMaxLength;
    public int TypingModeMaxLength
    {
        get => _typingModeMaxLength;
        set
        {
            _typingModeMaxLength = value;
            OnPropertyChanged();
        }
    }

    private int _typingModeMinLength;
    public int TypingModeMinLength
    {
        get => _typingModeMinLength;
        set
        {
            _typingModeMinLength = value;
            OnPropertyChanged();
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(Title):
                titleLabel.Text = Title;
                typingModeLabel.Text = Title;
                break;
            case nameof(IsTypingMode):
                if (IsTypingMode)
                {
                    titleLabel.IsVisible = false;
                    typingModeLabel.IsVisible = true;
                    typingModeEntry.IsVisible = true;
                    typingModeSubmitButton.IsVisible = true;
                    typingModeClearButton.IsVisible = true;
                }
                else
                {
                    titleLabel.IsVisible = true;
                    typingModeLabel.IsVisible = false;
                    typingModeEntry.IsVisible = false;
                    typingModeSubmitButton.IsVisible = false;
                    typingModeClearButton.IsVisible = false;
                }
                break;
        }
    }

    internal void ClearTypingModeText()
    {
        typingModeEntry.Text = "";
    }

    protected void LetterButtonClicked(object? sender, EventArgs e)
    {
        var selectedLetter = ((VisualElement)sender).StyleId;
        var isTeacherButton = false;

        if (selectedLetter.Equals("TEACHER", StringComparison.CurrentCultureIgnoreCase))
        {
            selectedLetter = "";
            isTeacherButton = true;
        }

        // no need to trigger consuming code's event handler if the buttons are just typing to the textbox
        if (isTeacherButton || !IsTypingMode)
        {
            var letterButtonEventArgs = new LetterButtonClickEventArgs()
            {
                SelectedLetter = selectedLetter,
                IsEmployeeButton = isTeacherButton
            };

            if(LetterButtonClickHandler != null) {
                LetterButtonClickHandler.Invoke(this, letterButtonEventArgs);
            }
        }
        else if (typingModeEntry.Text.Length < TypingModeMaxLength)
        {
            typingModeEntry.Text += selectedLetter;
        }
    }

    protected void typingModeSubmitButton_Clicked(object? sender, EventArgs e)
    {
        //todo: get value from box.  maybe no min prop, let consumer validate. just use max to stop further entry
        var submitButtonClickEventArgs = new TypingModeSubmitButtonClickEventArgs() { EnteredText = typingModeEntry.Text };
        if(TypingModeSubmitButtonClickHandler != null){TypingModeSubmitButtonClickHandler.Invoke(this, submitButtonClickEventArgs);}
    }
    public void AddToGrid()
    {
        titleLabel.FontSize = LetterButtonFontSize;
        typingModeLabel.FontSize = LetterButtonFontSize;
        titleLabel.TextColor = ConstantsStatics.GoddardMediumColor;
        typingModeLabel.TextColor = ConstantsStatics.GoddardMediumColor;
        titleLabel.FontAttributes = FontAttributes.Bold;
        titleLabel.Margin = new Thickness(6, 0, 0, 0);
        typingModeLabel.FontAttributes = FontAttributes.Bold;
        titleLabel.VerticalOptions = LayoutOptions.Center;
        titleLabel.HorizontalOptions = LayoutOptions.Start;
        typingModeLabel.TextColor = Colors.White;
        typingModeLabel.IsVisible = false;
        typingModeLabel.Margin = new Thickness(5, 0, 0, 20);
        typingModeLabel.VerticalOptions = LayoutOptions.Center;
        typingModeLabel.HorizontalOptions = LayoutOptions.Start;
        typingModeEntry.BackgroundColor = Colors.White;
        typingModeEntry.IsVisible = false;
        typingModeEntry.Margin = new Thickness(8, 4, 8, 24);
        typingModeEntry.FontAttributes = FontAttributes.Bold;
        typingModeEntry.FontSize = 28;
        typingModeEntry.Text = "";
        typingModeEntry.TextColor = ConstantsStatics.GoddardMediumColor;
        typingModeEntry.IsEnabled = false;
        typingModeEntry.HeightRequest = 50;
        typingModeEntry.WidthRequest = 300;
        typingModeClearButton.Text = "CLEAR";
        typingModeClearButton.Margin = new Thickness(0, 0, 0, 20);
        typingModeClearButton.Clicked += typingModeClearButton_Clicked;
        typingModeSubmitButton.Text = "OK";
        typingModeSubmitButton.Margin = new Thickness(0, 0, 0, 20);
        typingModeSubmitButton.Clicked += typingModeSubmitButton_Clicked;
        typingModeSubmitButton.FontSize = LetterButtonFontSize;
        typingModeClearButton.FontSize = LetterButtonFontSize;
        textBoxGrid.AddWithSpan(titleLabel, 0, 0, 1, 1);
        textBoxGrid.AddWithSpan(typingModeLabel, 0, 1, 1, 1);
        textBoxGrid.AddWithSpan(typingModeEntry, 0, 2, 1, 2);
        textBoxGrid.AddWithSpan(typingModeSubmitButton, 0, 4, 1, 1);
        textBoxGrid.AddWithSpan(typingModeClearButton, 0, 5, 1, 1);
        CreateButtons();
    }
    public void CreateButtons()
    {
        letterButtonsGrid.ColumnDefinitions.Clear();
        letterButtonsGrid.RowDefinitions.Clear();
        letterButtonsGrid.Children.Clear();
        string[] alphabet = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "TEACHER"];

        int rowCount = alphabet.Length / ColumnCount;
        for (int i = 0; i < ColumnCount; i++)
        {
            letterButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < rowCount; i++)
        {
            letterButtonsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < alphabet.Length; i++)
        {
            int rowPlace = i / ColumnCount;
            int columnPlace = i % ColumnCount;
            if (alphabet[i] != "TEACHER")
            {
                GoddardButton button = new GoddardButton
                {
                    Text = alphabet[i].ToString(),
                    StyleId = alphabet[i].ToString(),
                    Padding = new Thickness(5),
                    WidthRequest = LetterButtonSize,
                    HeightRequest = LetterButtonSize,
                    FontSize = LetterButtonFontSize
                };
                letterButtonsGrid.AddWithSpan(button, rowPlace, columnPlace, 1, 1);
                button.Clicked += LetterButtonClicked;

            }
            else
            {
                GoddardImageButton button = new GoddardImageButton
                {
                    Source = "apple_icon.png",
                    StyleId = "TEACHER",
                    UseAltColor = true,
                    Padding = new Thickness(5),
                    WidthRequest = LetterButtonSize,
                    HeightRequest = LetterButtonSize,
                    Aspect = Aspect.AspectFit
                };
                letterButtonsGrid.AddWithSpan(button, rowPlace, columnPlace, 1, 1);

                button.Clicked += LetterButtonClicked;
            };


        }

    }



    private void typingModeClearButton_Clicked(object? sender, EventArgs e)
    {
        ClearTypingModeText();
    }

    public class LetterButtonClickEventArgs : EventArgs
    {
        public string? SelectedLetter { get; set; }
        public bool IsEmployeeButton { get; set; }
    }

    public class TypingModeSubmitButtonClickEventArgs : EventArgs
    {
        public string? EnteredText { get; set; }
    }

}
