using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Media.SpeechSynthesis;
using Windows.System.Profile;
using System.ComponentModel;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeatherTalk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        public string DeviceFamily = "";
        public string Dimensions { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        // Define a member variable for storing the signed-in user. 
        //private MobileServiceUser user;

        // Define the OpenWeatherMap service object for REST weather data calls
        OpenWeatherMapService owms;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            owms = new OpenWeatherMapService();
            
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons.BackPressed"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                //UIViewSettings uiv = new UIViewSettings();
                string uim = UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
                DeviceFamily = "Device Family: " + AnalyticsInfo.VersionInfo.DeviceFamily + " (" + uim + ")";
                this.SizeChanged += MainPage_SizeChanged;
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentWidth = Window.Current.Bounds.Width;
            var currentHeight = Window.Current.Bounds.Height;
            Dimensions = string.Format(
              "Current Window Size: {0} x {1}",
              (int)currentWidth,
              (int)currentHeight);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Dimensions)));
            }
        }

        private async void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                // Do something with the back button here
                var dlg = new Windows.UI.Popups.MessageDialog("You tapped the Back button!!!.");
                await dlg.ShowAsync();
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            // Let's authenticate the user via Twitter OAuth
            //await AuthenticateAsync();
            string location = e.Parameter as string;
            if (location.Length > 0)
            {
                ShowWeather(location);
            }
        }

        private async void ButtonLookup_Click(object sender, RoutedEventArgs e)
        {
            string location = txtLocation.Text.Trim();

            if (location.Length == 0)
            {
                var dlg = new Windows.UI.Popups.MessageDialog("Make sure you provide a city name and state before asking for a weather report.");
                await dlg.ShowAsync();
            }

            ShowWeather(location);
        }

        private async void ShowWeather(string location)
        {
            prgActivity.IsActive = true;
            try
            {
                //if (user == null)
                //{
                //    var dlg = new Windows.UI.Popups.MessageDialog("Not so fast... You need to be authenticated to use this app.");
                //    await dlg.ShowAsync();
                //    return;
                //}

                App.TelemetryClient.TrackEvent("ShowWeather", new Dictionary<string, string>()
                {
                    { "Location", location }
                });

                var wr = await owms.GetWeather(location);
                if (wr != null)
                {
                    var weatherText = "The current temperature in {0} is {1}°F, with a high today of {2}° and a low of {3}°.";
                    string weatherMessage = string.Format(weatherText, wr.name, (int)wr.main.temp, (int)wr.main.temp_max, (int)wr.main.temp_min);
                    lblTempHigh.Text = string.Format("High: {0}°F", (int)wr.main.temp_max);
                    lblTempLow.Text = string.Format("Low: {0}°F", (int)wr.main.temp_min);
                    lblTemp.Text = string.Format("{0}°", (int)wr.main.temp);
                    lblLocation.Text = wr.name;
                    ReadText(weatherMessage);

                    // Save this as a favorite city in Azure
                    //Location favorite = new Location { Name = wr.name };
                    //Location favorite = new Location { Name = wr.name, UserId = user.UserId };
                    //await App.MobileService.GetTable<Location>().InsertAsync(favorite);
                }
            }
            catch (Exception exc)
            {
                var dlg = new Windows.UI.Popups.MessageDialog("Opps! Something went wrong getting the latest weather info. That can't be good...");
                await dlg.ShowAsync();
            }
            prgActivity.IsActive = false;
        }

        private async void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Windows.UI.Popups.MessageDialog("This feature is not implemented yet.", "Help");
            await dlg.ShowAsync();
        }

        private async void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Windows.UI.Popups.MessageDialog("This feature is not implemented yet.", "Settings");
            await dlg.ShowAsync();
        }

        // Define a method that performs the authentication process
        // using a Twitter sign-in. 
        //private async System.Threading.Tasks.Task AuthenticateAsync()
        //{
        //    while (user == null)
        //    {
        //        string message;
        //        try
        //        {
        //            // Change 'MobileService' to the name of your MobileServiceClient instance.
        //            // Sign-in using Facebook authentication.
        //            user = await App.MobileService
        //                .LoginAsync(MobileServiceAuthenticationProvider.Twitter);
        //            message =
        //                string.Format("You are now signed in - {0}", user.UserId);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            message = "Nice try, but an authenticated login is required.";
        //        }

        //        var dialog = new Windows.UI.Popups.MessageDialog(message);
        //        dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
        //        await dialog.ShowAsync();
        //    }
        //}

        // Quickly adds Text-to-Speech to the app using Cortana's default voice
        private async void ReadText(string mytext)
        {
            //Reminder: You need to enable the Microphone capabilitiy in Windows Phone projects
            //Reminder: Add this namespace in your using statements
            //using Windows.Media.SpeechSynthesis;

            // The media object for controlling and playing audio.
            MediaElement mediaplayer = new MediaElement();

            // The object for controlling the speech synthesis engine (voice).
            using (var speech = new SpeechSynthesizer())
            {
                //Retrieve the first female voice
                speech.Voice = SpeechSynthesizer.AllVoices
                    .First(i => (i.Gender == VoiceGender.Female && i.Description.Contains("United States")));
                // Generate the audio stream from plain text.
                SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync(mytext);

                // Send the stream to the media object.
                mediaplayer.SetSource(stream, stream.ContentType);
                mediaplayer.Play();
            }
        }
    }
}
