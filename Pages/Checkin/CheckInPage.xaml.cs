using Goddard.Clock.Helpers;
using Goddard.Clock.Models;
using Goddard.Clock.Data;
using CommunityToolkit.Maui.Views;

namespace Goddard.Clock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckInPage : TimedContentPage
    {
        public const int MAX_PERSON_COUNT = 6;
        private bool _isSigRequired;
        private readonly ClockDatabase _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        private readonly NavigationService _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));

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

                    _ = DisplayAlert("Error", String.Format("1 to {0} persons to check in/out expected.", MAX_PERSON_COUNT), "OK");
                }

                IsEmployeeClockEvent = ClockEvents.First().UserType == UserType.Employee;

                // note that if employee is checking in/out children, we purposely check BypassSignatureParents, not BypassSignatureEmployees
                if (IsEmployeeClockEvent)
                {
                    instructionsContainer.IsVisible = Settings.BypassSignatureEmployees;
                    drawingView.IsVisible = !Settings.BypassSignatureEmployees;
                    _isSigRequired = !Settings.BypassSignatureEmployees;
                }
                else
                {
                    instructionsContainer.IsVisible = Settings.BypassSignatureParents;
                    drawingView.IsVisible = !Settings.BypassSignatureParents;
                    _isSigRequired = !Settings.BypassSignatureParents;
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

                    eventsGrid.Add(new Label() { Text = nameLabelText, Margin = new Thickness(0, 0, 25, 0) }, nameLabelCol, nameLabelRow);
                    eventsGrid.Add(new Label() { Text = timeLabelText, FontAttributes = FontAttributes.Italic | FontAttributes.Bold }, timeLabelCol, timeLabelRow);
                }
            }
            catch (Exception ex)
            {
                _ = Logging.Log(_database, ex);
                throw;
            }
        }

        void Clear_Button_Clicked(object sender, EventArgs e)
        {
            drawingView.Clear();
        }

        protected async void okFooterButtonClick(object? sender, EventArgs e)
        {
            try
            {
                if (_isSigRequired && !drawingView.Lines.Any())
                {
                    await DisplayAlert("", "please sign the signature pad", "OK");
                    return;
                }

                byte[] signatureBYTES = new byte[] { };

                if (drawingView.Lines.Any())
                {
                    var imageStream = await drawingView.GetImageStream(300, 300);

                    var signatureMemoryStream = imageStream as MemoryStream;
                    if (signatureMemoryStream == null)
                    {
                        signatureMemoryStream = new MemoryStream();
                        imageStream.CopyTo(signatureMemoryStream);
                    }

                    signatureBYTES = signatureMemoryStream.ToArray();
                }

                var events = new List<Event>();
                foreach (var ex in ClockEvents)
                {
                    var myEvent = ex.ConvertToEvent();
                    if (drawingView.Lines.Any())
                    {
                        myEvent.Signature = signatureBYTES;
                    }
                    myEvent.Occurred = DateTime.Now;
                    events.Add(myEvent);
                }

                var savedResults = await _database.EnterClockEvents(events);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var msg = savedResults ? "sign in/out successful" : "sign in/out failed";
                    var modalUserMessage = new ModalUserMessage(_navigation, _database, msg, false, false, 4, true);
                    modalUserMessage.Show();
                });
            }
            catch (Exception ex)
            {
                _ = Logging.Log(_database, ex);
                throw;
            }
        }
    }
}
