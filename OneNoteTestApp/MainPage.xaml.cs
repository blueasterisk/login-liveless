using OneNoteTestApp.Helpers;
using OneNoteTestApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Security.Authentication.OnlineId;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace OneNoteTestApp
{
    public sealed partial class MainPage : Page
    {
        #region Private fields
        OnlineIdAuthenticator _authenticator;
        string _accessToken;
        private MainPage _current;
        static string URI_API_LIVE = "https://apis.live.net/v5.0/";
        static string URI_PICTURE = URI_API_LIVE + "me/picture?access_token=";
        static string URI_CONTACTS = URI_API_LIVE + "me/contacts?access_token=";
        static string URI_USER_INFO = URI_API_LIVE + "me?access_token=";
        #endregion

        #region Props

        public MainPage Current
        {
            get
            {
                if (_current == null)
                    return new MainPage();
                else
                    return _current;
                 
            }
            set
            {
                _current = value;
            }
        }

        public Boolean NeedsToGetTicket
        {
            get;
            private set;
        }

        public Boolean CanSignOut
        {
            get { return _authenticator.CanSignOut; }
        }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }

            private set
            {
                if (_accessToken != value)
                {
                    _accessToken = value;

                    if (value != null)
                    {
                        UserPicture.Source = new BitmapImage(new Uri(URI_PICTURE + value));

                        GetUserInfo(value);
                        GetUserContacts(value);
                    }
                    else
                    {
                        UserName.Text = "";
                    }
                }
            }
        }

        public CredentialPromptType PromptType
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        public async void GetUserInfo(string token)
        {
            var uri = new Uri(URI_USER_INFO + token);
            var client = new HttpClient();
            var result = await client.GetAsync(uri);

            string jsonUserInfo = await result.Content.ReadAsStringAsync();
            if (jsonUserInfo != null)
            {
                var json = JsonObject.Parse(jsonUserInfo);
                UserName.Text = "Welcome, " + json["name"].GetString();
            }
        }
        public async void GetUserContacts(string token)
        {
            Uri[] _localImages = 
            {
                new Uri("ms-appx:///Images/user.png"),
                new Uri("ms-appx:///SampleData/Images/60Mail01.png"),
                new Uri("ms-appx:///SampleData/Images/60Mail02.png"),
                new Uri("ms-appx:///SampleData/Images/60Mail03.png"),
                new Uri("ms-appx:///SampleData/Images/msg.png"),
            };

            var uri = new Uri(URI_CONTACTS + token);
            var client = new HttpClient();
            var result = await client.GetAsync(uri);
            string jsonUserContacts = await result.Content.ReadAsStringAsync();
            if (jsonUserContacts != null)
            {
                var json = JsonObject.Parse(jsonUserContacts);
                var contacts = json["data"] as JsonValue;
                var storeData = new StoreData();
                int index = 0;

                foreach (JsonValue contact in contacts.GetArray())
                {
                    var obj = contact.GetObject();
                    var item = new Item();
                    item.Name = obj["name"].GetString();
                    item.Id = obj["id"].GetString();

                    if (obj["user_id"].ValueType == JsonValueType.String)
                    {
                        string userId = obj["user_id"].GetString();
                        item.SetImage(URI_API_LIVE + userId + "/picture?access_token=" + token);
                    }
                    else
                    {
                        item.Image = new BitmapImage(_localImages[index % _localImages.Length]);
                    }

                    storeData.Collection.Add(item);
                    index++;
                }
            }
        }

        private void DebugPrint(String trace)
        {
            DebugArea.Text = trace;
        }
        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            _authenticator = new OnlineIdAuthenticator();
            PromptType = CredentialPromptType.PromptIfNeeded;
            AccessToken = null;
            NeedsToGetTicket = true;
        }

        #endregion

        #region Event Handlers

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var targetArray = new List<OnlineIdServiceTicketRequest>();
            targetArray.Add(new OnlineIdServiceTicketRequest("wl.basic wl.contacts_photos office.onenote_create", "DELEGATION"));

            AccessToken = null;
            NeedsToGetTicket = true;

            DebugPrint("Signing in...");

            try
            {
                var result = await _authenticator.AuthenticateUserAsync(targetArray, PromptType);
                if (result.Tickets[0].Value != string.Empty)
                {
                    DebugPrint("Signed in.");

                    NeedsToGetTicket = false;
                    AccessToken = result.Tickets[0].Value;
                }
                else
                {
                    // errors are to be handled here.
                    DebugPrint("Unable to get the ticket. Error: " + result.Tickets[0].ErrorCode.ToString());
                }
            }
            catch (System.OperationCanceledException)
            {
                // errors are to be handled here.
                DebugPrint("Operation canceled.");

            }
            catch (System.Exception ex)
            {
                // errors are to be handled here.
                DebugPrint("Unable to get the ticket. Exception: " + ex.Message);
            }

            SignInButton.Visibility = NeedsToGetTicket ? Visibility.Visible : Visibility.Collapsed;
            GoNextStackpanel.Visibility = NeedsToGetTicket ? Visibility.Collapsed : Visibility.Visible;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomePage),Current);
        }

        #endregion

    }
}

