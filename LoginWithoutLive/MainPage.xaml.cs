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


namespace LoginWithoutLive
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Private fields
        OnlineIdAuthenticator _authenticator;
        string _accessToken;
        static string URI_API_LIVE = "https://apis.live.net/v5.0/";
        static string URI_PICTURE = URI_API_LIVE + "me/picture?access_token=";
        static string URI_CONTACTS = URI_API_LIVE + "me/contacts?access_token=";
        static string URI_USER_INFO = URI_API_LIVE + "me?access_token=";
        #endregion

        #region Props

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
                        cvs1.Source = null;
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

        #region public classes
        public class Item : System.ComponentModel.INotifyPropertyChanged
        {
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }

            private string _Name = string.Empty;
            public string Name
            {
                get
                {
                    return this._Name;
                }

                set
                {
                    if (this._Name != value)
                    {
                        this._Name = value;
                        this.OnPropertyChanged("Name");
                    }
                }
            }

            private string _Id = string.Empty;
            public string Id
            {
                get
                {
                    return this._Id;
                }

                set
                {
                    if (this._Id != value)
                    {
                        this._Id = value;
                        this.OnPropertyChanged("Id");
                    }
                }
            }

            private ImageSource _Image = null;
            public ImageSource Image
            {
                get
                {
                    return this._Image;
                }

                set
                {
                    if (this._Image != value)
                    {
                        this._Image = value;
                        this.OnPropertyChanged("Image");
                    }
                }
            }

            public void SetImage(String path)
            {
                Image = new BitmapImage(new Uri(path));
            }

            private string _Link = string.Empty;
            public string Link
            {
                get
                {
                    return this._Link;
                }

                set
                {
                    if (this._Link != value)
                    {
                        this._Link = value;
                        this.OnPropertyChanged("Link");
                    }
                }
            }
        }

        public class GroupInfoList<T> : List<object>
        {

            public object Key { get; set; }


            public new IEnumerator<object> GetEnumerator()
            {
                return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
            }
        }

        public class StoreData
        {
            public StoreData()
            {
            }

            private ItemCollection _Collection = new ItemCollection();

            public ItemCollection Collection
            {
                get
                {
                    return this._Collection;
                }
            }

            internal List<GroupInfoList<object>> GetGroupsByLetter()
            {
                List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();

                var query = from item in Collection
                            orderby ((Item)item).Name
                            group item by ((Item)item).Name[0] into g
                            select new { GroupName = g.Key, Items = g };
                foreach (var g in query)
                {
                    GroupInfoList<object> info = new GroupInfoList<object>();
                    info.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        info.Add(item);
                    }
                    groups.Add(info);
                }

                return groups;
            }
        }

        public class ItemCollection : IEnumerable<Object>
        {
            private System.Collections.ObjectModel.ObservableCollection<Item> itemCollection = new System.Collections.ObjectModel.ObservableCollection<Item>();

            public IEnumerator<Object> GetEnumerator()
            {
                return itemCollection.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(Item item)
            {
                itemCollection.Add(item);
            }
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
                cvs1.Source = storeData.GetGroupsByLetter();
            }
        }

        private void DebugPrint(String trace)
        {
            DebugArea.Text = trace;
        }
        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

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
        }
        #endregion
    }
}
