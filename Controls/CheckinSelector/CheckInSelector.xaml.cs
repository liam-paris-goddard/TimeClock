﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeClock.Models;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    public partial class CheckInSelector : ContentView
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


        public ClockEventType? SelectedEventType { get; private set; }

        public string FullName
        {
            get => $"{LastName}, {FirstName}";
            internal set { }
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

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
            }
        }

        private void checkInButton_Clicked(object? sender, EventArgs? e)
        {
            if (SelectedEventType == ClockEventType.In)
            {
                SelectedEventType = null;
            }
            else
            {
                SelectedEventType = ClockEventType.In;
                this.SelectionMade?.Invoke(this, e);
            }

            UpdateButtonColors();
        }

        private void checkOutButton_Clicked(object? sender, EventArgs? e)
        {
            if (SelectedEventType == ClockEventType.Out)
            {
                SelectedEventType = null;
            }
            else
            {
                SelectedEventType = ClockEventType.Out;
                this.SelectionMade?.Invoke(this, e);
            }

            UpdateButtonColors();
        }

        private void UpdateButtonColors()
        {
            checkInButton.UseAltColor = SelectedEventType == ClockEventType.In;
            checkOutButton.UseAltColor = SelectedEventType == ClockEventType.Out;
        }

        public void ChangeSelectedEventType(ClockEventType eventType)
        {
            if (SelectedEventType != eventType)
            {
                if (eventType == ClockEventType.In)
                    checkInButton_Clicked(null, null);
                else if (eventType == ClockEventType.Out)
                    checkOutButton_Clicked(null, null);
            }
        }
    }
}