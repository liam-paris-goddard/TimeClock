using TimeClock.Helpers;
using TimeClock.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckInPage : TimedContentPage
    {
        public const int MAX_PERSON_COUNT = 6;
        private bool _isSigRequired;

        public List<EventExtended> ClockEvents { get; set; }
        public long UserID { get; set; }
        public bool IsEmployeeClockEvent { get; set; }

        public CheckInPage(List<EventExtended> clockEvents)
        {
            try
            {
                InitializeComponent();

                ClockEvents = clockEvents;

                if (ClockEvents.Count < 1 || ClockEvents.Count > MAX_PERSON_COUNT)
                {
                    //TODO: log / throw ex?  not sure yet how we are handling errors
                    DisplayAlert("Error", String.Format("1 to {0} persons to check in/out expected.", MAX_PERSON_COUNT), "OK");
                }

                IsEmployeeClockEvent = ClockEvents.First().UserType == UserType.Employee;

                // note that if employee is checking in/out children, we purposely check BypassSignatureParents, not BypassSignatureEmployees
                if (IsEmployeeClockEvent)
                {
                    instructionsContainer.IsVisible = Helpers.Settings.BypassSignatureEmployees;
                    drawingView.IsVisible = !Helpers.Settings.BypassSignatureEmployees;
                    _isSigRequired = !Helpers.Settings.BypassSignatureEmployees;
                }
                else
                {
                    instructionsContainer.IsVisible = Helpers.Settings.BypassSignatureParents;
                    drawingView.IsVisible = !Helpers.Settings.BypassSignatureParents;
                    _isSigRequired = !Helpers.Settings.BypassSignatureParents;
                }


                // these are the column/row positions in the order that we will want to assign to each of the grid labels
                var gridPositions = new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(0, 0),
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(0, 1),
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(0, 2),
                    new Tuple<int, int>(1, 2),
                    new Tuple<int, int>(2, 0),
                    new Tuple<int, int>(3, 0),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(2, 2),
                    new Tuple<int, int>(3, 2)
                };

                for (int i = 0; i < ClockEvents.Count && i < MAX_PERSON_COUNT; i++)
                {
                    var nameLabelCol = gridPositions[i * 2].Item1;
                    var nameLabelRow = gridPositions[i * 2].Item2;
                    var timeLabelCol = gridPositions[i * 2 + 1].Item1;
                    var timeLabelRow = gridPositions[i * 2 + 1].Item2;
                    var nameLabelText = ClockEvents[i].TargetPersonName;
                    var timeLabelText = String.Format("{0}  {1}", ClockEvents[i].Type == ClockEventType.Out ? "CLOCK OUT" : "CLOCK IN", DateTime.Now.ToString("h:mm tt"));

                    eventsGrid.Children.Add(new Label() { Text = nameLabelText, Margin = new Thickness(0, 0, 25, 0) }, nameLabelCol, nameLabelRow);
                    eventsGrid.Children.Add(new Label() { Text = timeLabelText, FontAttributes = FontAttributes.Italic | FontAttributes.Bold }, timeLabelCol, timeLabelRow);
                }
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                throw;
            }
        }

        protected async void okFooterButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_isSigRequired && !drawingView.Points.Any())
                {
                    await DisplayAlert("", "please sign the signature pad", "OK");
                    return;
                }

                byte[] signatureBYTES = new byte[] { };

                if (drawingView.Points.Any())
                {
                    var imageSource = await drawingView.ToImageSourceAsync();
                    var streamImageSource = (StreamImageSource)imageSource;
                    var stream = await streamImageSource.Stream(CancellationToken.None);
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        signatureBYTES = memoryStream.ToArray();
                    }
                }

                var events = new List<Event>();
                foreach (var ex in ClockEvents)
                {
                    var myEvent = ex.ConvertToEvent();
                    if (!drawingView.Points.Any())
                    {
                        myEvent.Signature = signatureBYTES;
                    }
                    myEvent.Occurred = DateTime.Now;
                    events.Add(myEvent);
                }

                var savedResults = await App.Database.EnterClockEvents(events);

                Device.BeginInvokeOnMainThread(() =>
                {
                    var msg = savedResults ? "sign in/out successful" : "sign in/out failed";
                    var modalUserMessage = new ModalUserMessage(msg, false, false, 4, true);
                    modalUserMessage.Show();
                });
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                throw;
            }
        }
    }
}
