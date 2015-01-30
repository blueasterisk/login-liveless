using Newtonsoft.Json;
using OneNoteTestApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace OneNoteTestApp.Views
{
    public sealed partial class HomePage : Page
    {
        #region Private fields
        private static string _accessToken;
        
        #endregion

        #region Constructor
        
        public HomePage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Event Handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _accessToken = (e.Parameter as MainPage).AccessToken;
            base.OnNavigatedTo(e);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ApiBaseResponse response = await CreateSimplePage(HeadingBox.Text, BodyBox.Text);
            StatusTextBox.Text = "The Note can be accessed via\n " + response.Links.OneNoteWebUrl.Href.ToString() + "\n" + response.Body.ToString();
        }

        #endregion

        #region Methods
        public static async Task<ApiBaseResponse> CreateSimplePage(string heading, string body)
        {
            var client = new HttpClient();

            // Note: API only supports JSON response.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Not adding the Authentication header would produce an unauthorized call and the API will return a 401
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            string date = GetDate();
            string simpleHtml = "<html>" +
                                "<head>" +
                                "<title>" + heading + "</title>" +
                                "<meta name=\"created\" content=\"" + date + "\" />" +
                                "</head>" +
                                "<body>" +
                                body +
                                "</body>" +
                                "</html>";

            // Prepare an HTTP POST request to the Pages endpoint
            // The request body content type is text/html
            var createMessage = new HttpRequestMessage(HttpMethod.Post, @"https://www.onenote.com/api/v1.0/pages")
            {
                Content = new StringContent(simpleHtml, Encoding.UTF8, "text/html")
            };

            HttpResponseMessage response = await client.SendAsync(createMessage);

            return await TranslateResponse(response);
        }
        private static async Task<ApiBaseResponse> TranslateResponse(HttpResponseMessage response)
        {
            ApiBaseResponse apiBaseResponse;
            string body = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.Created
                /* POST Page calls always return 201-Created upon success */)
            {
                apiBaseResponse = JsonConvert.DeserializeObject<PageResponse>(body);
            }
            else
            {
                apiBaseResponse = new ApiBaseResponse();
            }

            // Extract the correlation id.  Apps should log this if they want to collect the data to diagnose failures with Microsoft support 
            IEnumerable<string> correlationValues;
            if (response.Headers.TryGetValues("X-CorrelationId", out correlationValues))
            {
                apiBaseResponse.CorrelationId = correlationValues.FirstOrDefault();
            }
            apiBaseResponse.StatusCode = response.StatusCode;
            apiBaseResponse.Body = body;
            return apiBaseResponse;
        }
        private static string GetDate()
        {
            return DateTime.Now.ToString("o");
        }
        #endregion
    }
}
