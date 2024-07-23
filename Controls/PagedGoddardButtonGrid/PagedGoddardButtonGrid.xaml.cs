using Microsoft.Maui.Devices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Goddard.Clock;

namespace Goddard.Clock.Controls
{
    public class PagedGoddardButtonGridItem
    {
        public string? Text;
        public string? Value;
    }


    public partial class PagedGoddardButtonGrid : BaseContentView
    {
        private int ColumnCount;
        private int RowCount;
        private double _buttonFontSize;
        public double ButtonFontSize
        {
            get => _buttonFontSize;
            set
            {
                _buttonFontSize = value;
                OnPropertyChanged();
            }
        }

        private double _buttonHeight;
        public double ButtonHeight
        {
            get => _buttonHeight;
            set
            {
                _buttonHeight = value;
                OnPropertyChanged();
            }
        }

        public double _buttonWidth;
        public double ButtonWidth
        {
            get => _buttonWidth;
            set
            {
                _buttonWidth = value;
                OnPropertyChanged();
            }
        }

        private const string PreviousButtonValue = "[[NAV_PREVIOUS]]";
        private const string NextButtonValue = "[[NAV_NEXT]]";

        public static readonly BindableProperty ItemsProperty =
            BindableProperty.Create(nameof(Items), typeof(List<PagedGoddardButtonGridItem>), typeof(PagedGoddardButtonGrid), default, propertyChanged: ItemsProperty_Changed);

        public event EventHandler<ButtonClickEventArgs>? ButtonClickHandler;

        protected override void OnDeviceInformationChanged(string propertyName)
        {
            if (propertyName == "DeviceWidth" || propertyName == "GlobalOrientation" || propertyName == "DeviceHeight")
            {
                SetResponsiveVars();
                updatePagedContent();
                CreateButtons();
            }

        }


        public static void ItemsProperty_Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var pagedGoddardButtonGrid = (PagedGoddardButtonGrid)bindable;
            pagedGoddardButtonGrid.InitializePagedData();

        }

        public List<PagedGoddardButtonGridItem> Items
        {
            get => (List<PagedGoddardButtonGridItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        private Dictionary<int, List<PagedGoddardButtonGridItem>> _pagedItems = new Dictionary<int, List<PagedGoddardButtonGridItem>>();

        protected int PageNumber { get; set; }

        public PagedGoddardButtonGrid()
        {
            SetResponsiveVars();
            InitializeComponent();
        }
        private void InitializePagedData()
        {
            SetResponsiveVars();
            updatePagedContent();  
            CreateButtons();
        }

       public void updatePagedContent() {
            _pagedItems.Clear();
            int myPageNumber = 0;
            _pagedItems.Add(0, new List<PagedGoddardButtonGridItem>());

            foreach (var item in Items)
            {
                var myPageItems = _pagedItems[myPageNumber];


                if (myPageItems.Count == (ColumnCount * RowCount - 1) && item != Items.Last())
                {
                    myPageItems.Add(new PagedGoddardButtonGridItem() { Value = NextButtonValue, Text = "Next" });

                    myPageNumber++;
                    _pagedItems.Add(myPageNumber, new List<PagedGoddardButtonGridItem>());
                    myPageItems = _pagedItems[myPageNumber];

                    myPageItems.Add(new PagedGoddardButtonGridItem() { Value = PreviousButtonValue, Text = "Previous" });
                }

                myPageItems.Add(item);
            }
        }

        private void SetResponsiveVars()
        {
            ColumnCount = 5;
            RowCount = 5;
            ButtonFontSize = 15;
            ButtonHeight = 60;
            ButtonWidth = 180;
            if (DeviceType == "small")
            {
                if (DeviceOrientation == DisplayOrientation.Portrait)
                {
                    ColumnCount = 3;
                    RowCount = 7;
                    //ButtonHeight = 70;
                    //ButtonWidth
                    //ButtonFontSize
                }
                else
                {
                    ColumnCount = 5;
                    RowCount = 4;
                    //ButtonHeight
                    //ButtonWidth
                    //ButtonFontSize
                }
            }
            else 
            {
                if (DeviceOrientation == DisplayOrientation.Portrait)
                {
                    ColumnCount = 3;
                    RowCount = 7;
                } else
                {
                    ColumnCount = 5;
                    RowCount = 5;
                }
            }
        }

        private void CreateButtons()
        {
            buttonGrid.ColumnDefinitions.Clear();
            buttonGrid.RowDefinitions.Clear();
            buttonGrid.Children.Clear();
            for (int i = 0; i < ColumnCount; i++)
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < RowCount; i++)
                buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < _pagedItems[PageNumber].Count; i++)
            {
                var itemsInPage = _pagedItems[PageNumber];
                var rowNumber = i / ColumnCount;
                var columnNumber = i % ColumnCount;

                var buttonText = itemsInPage[i].Text;
                var buttonValue = itemsInPage[i].Value;

                var button = new GoddardButton()
                {
                    Text = buttonText,
                    StyleId = buttonValue,
                    FontSize = ButtonFontSize,
                    HeightRequest = ButtonHeight,
                    WidthRequest = ButtonWidth,
                };
                if (buttonValue == PreviousButtonValue)
                {
                    button.Clicked += NavButtonClick;
                    button.ImageSource = "back.png";
                }
                else if (buttonValue == NextButtonValue)
                {
                    button.Clicked += NavButtonClick;
                    button.ImageSource = "more.png";
                    button.ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10); // ?
                }
                else
                {
                    button.Clicked += ButtonClick;
                }

                buttonGrid.Add(button, columnNumber, rowNumber);
            }
        }

        private void NavButtonClick(object? sender, EventArgs e)
        {
            // iOS crashes when the nav button tries to remove itself, so we're just hiding the current buttons, then on subsequent page changes, 
            // this for statement removes the old hidden buttons so they don't build up with each page navigation.  
            /*for (int i = buttonGrid.Children.Count() - 1; i >= 0; i--)
            {
                if (!buttonGrid.Children[i].IsVisible)
                    buttonGrid.Children.RemoveAt(i);
            }

            foreach (var child in buttonGrid.Children)
                child.IsVisible = false;
*/
            buttonGrid.Children.Clear();
            var button = (GoddardButton)sender;

            if (button.StyleId == PreviousButtonValue)
                PageNumber--;
            else if (button.StyleId == NextButtonValue)
                PageNumber++;

            CreateButtons();
        }

        protected void ButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var selectedValue = button.StyleId;
                var selectedText = button.Text;

                var buttonClickEventArgs = new ButtonClickEventArgs() { SelectedValue = selectedValue, SelectedText = selectedText };
                ButtonClickHandler?.Invoke(this, buttonClickEventArgs);
            }
        }

        public class ButtonClickEventArgs : EventArgs
        {
            public string? SelectedValue { get; set; }
            public string? SelectedText { get; set; }
        }
    }
}