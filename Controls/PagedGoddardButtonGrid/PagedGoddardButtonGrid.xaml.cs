using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    [DebuggerDisplay("Text = {Text}, Value = {Value}")]
    public class PagedGoddardButtonGridItem
    {
        public string? Text;
        public string? Value;
    }

    public partial class PagedGoddardButtonGrid : View
    {
        private const int ColumnCount = 5;
        private const int RowCount = 5;
        private const string PreviousButtonValue = "[[NAV_PREVIOUS]]";
        private const string NextButtonValue = "[[NAV_NEXT]]";

        public static readonly BindableProperty ItemsProperty =
            BindableProperty.CreateProperty<PagedGoddardButtonGrid, List<PagedGoddardButtonGridItem>>(p => p.Items, default, propertyChanged: ItemsPropery_Changed);

        public event EventHandler<ButtonClickEventArgs> ButtonClickHandler;

        public static void ItemsPropery_Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var pagedGoddardButtonGrid = (PagedGoddardButtonGrid)bindable;
            pagedGoddardButtonGrid.InitializePagedData();
            pagedGoddardButtonGrid.CreateButtons();
        }

        public List<PagedGoddardButtonGridItem> Items
        {
            get => (List<PagedGoddardButtonGridItem>)ItemsProperty.GetValue(this);
            set => ItemsProperty.SetValue(this, value);
        }

        private Dictionary<int, List<PagedGoddardButtonGridItem>> _pagedItems = new Dictionary<int, List<PagedGoddardButtonGridItem>>();

        protected int PageNumber { get; set; }
        private void InitializePagedData()
        {
            var pageReadyItems = new List<PagedGoddardButtonGridItem>();

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

        private void CreateButtons()
        {
            for (int i = 0; i < _pagedItems[PageNumber].Count(); i++)
            {
                var itemsInPage = _pagedItems[PageNumber];
                var rowNumber = i / ColumnCount;
                var columnNumber = i % ColumnCount;

                var buttonText = itemsInPage[i].Text;
                var buttonValue = itemsInPage[i].Value;

                var button = new GoddardButton()
                {
                    Text = buttonText,
                    StyleId = buttonValue
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

                buttonGrid.Children.Add(button, columnNumber, columnNumber + 1, rowNumber, rowNumber + 1);
            }
        }

        private void NavButtonClick(object sender, EventArgs e)
        {
            for (int i = buttonGrid.Children.Count() - 1; i >= 0; i--)
            {
                if (!buttonGrid.Children[i].IsVisible)
                    buttonGrid.Children.RemoveAt(i);
            }

            foreach (var child in buttonGrid.Children)
                child.IsVisible = false;

            var button = (GoddardButton)sender;

            if (button.StyleId == PreviousButtonValue)
                PageNumber--;
            else if (button.StyleId == NextButtonValue)
                PageNumber++;

            CreateButtons();
        }

        protected void ButtonClick(object sender, EventArgs e)
        {
            var selectedValue = ((Button)sender).StyleId;
            var selectedText = ((Button)sender).Text;

            var buttonClickEventArgs = new ButtonClickEventArgs() { SelectedValue = selectedValue, SelectedText = selectedText };
            ButtonClickHandler?.Invoke(this, buttonClickEventArgs);
        }

        public class ButtonClickEventArgs : EventArgs
        {
            public string? SelectedValue { get; set; }
            public string? SelectedText { get; set; }
        }
    }
}
