using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    public partial class LetterButtons : ContentView
    {
        public event EventHandler<LetterButtonClickEventArgs> LetterButtonClickHandler;
        public event EventHandler<TypingModeSubmitButtonClickEventArgs> TypingModeSubmitButtonClickHandler;

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private bool _isTypingMode;
        public bool IsTypingMode
        {
            get { return _isTypingMode; }
            set
            {
                _isTypingMode = value;
                OnPropertyChanged();
            }
        }

        private int _typingModeMaxLength;
        public int TypingModeMaxLength
        {
            get { return _typingModeMaxLength; }
            set
            {
                _typingModeMaxLength = value;
                OnPropertyChanged();
            }
        }

        private int _typingModeMinLength;
        public int TypingModeMinLength
        {
            get { return _typingModeMinLength; }
            set
            {
                _typingModeMinLength = value;
                OnPropertyChanged();
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
                        typingModeEntryContainer.IsVisible = true;
                        typingModeSubmitButton.IsVisible = true;
                        typingModeClearButton.IsVisible = true;
                    }
                    else
                    {
                        titleLabel.IsVisible = true;
                        typingModeLabel.IsVisible = false;
                        typingModeEntryContainer.IsVisible = false;
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

        protected void LetterButtonClicked(object sender, EventArgs e)
        {
            var selectedLetter = ((Button)sender).StyleId;
            var isTeacherButton = false;

            if (selectedLetter.ToUpper() == "TEACHER")
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

                LetterButtonClickHandler?.Invoke(this, letterButtonEventArgs);
            }
            else if (typingModeEntry.Text.Length < TypingModeMaxLength)
            {
                typingModeEntry.Text += selectedLetter;
            }
        }

        protected void typingModeSubmitButton_Clicked(object sender, EventArgs e)
        {
            //xtodo: get value from box.  maybe no min prop, let consumer validate. just use max to stop further entry
            var submitButtonClickEventArgs = new TypingModeSubmitButtonClickEventArgs() { EnteredText = typingModeEntry.Text };
            TypingModeSubmitButtonClickHandler?.Invoke(this, submitButtonClickEventArgs);
        }

        private void typingModeClearButton_Clicked(object sender, EventArgs e)
        {
            ClearTypingModeText();
        }

        public class LetterButtonClickEventArgs : EventArgs
        {
            public string SelectedLetter { get; set; }
            public bool IsEmployeeButton { get; set; }
        }

        public class TypingModeSubmitButtonClickEventArgs : EventArgs
        {
            public string EnteredText { get; set; }
        }
    }
}

