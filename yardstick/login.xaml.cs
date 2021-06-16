using System.Windows;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.System.RemoteSystems;
using Newtonsoft.Json;
using RestSharp;

namespace yardstick
{
    public partial class login : Window
    {
        private Auth0Client client;
        public RestClient restClient = new RestClient("http://localhost:8180");
        private readonly string[] _connectionNames = new string[]{
            "google-oauth2",
            "discord",
        };
        
        public login(){
            
            InitializeComponent();
        }
        
        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId
            });

            var extraParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(connectionNameComboBox.Text))
                extraParameters.Add("connection", connectionNameComboBox.Text);

            if (!string.IsNullOrEmpty(audienceTextBox.Text))
                extraParameters.Add("audience", audienceTextBox.Text);

            DisplayResult(await client.LoginAsync(extraParameters: extraParameters));
        }

        private void DisplayResult(LoginResult loginResult)
        {
            // Display error
            if (loginResult.IsError)
            {
                resultTextBox.Text = loginResult.Error;
                return;
            }
            
            
            logoutButton.Visibility = Visibility.Visible;
            loginButton.Visibility = Visibility.Collapsed;

            // Display result
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Tokens");
            sb.AppendLine("------");
            sb.AppendLine($"id_token: {loginResult.IdentityToken}");
            sb.AppendLine($"access_token: {loginResult.AccessToken}");
            sb.AppendLine($"refresh_token: {loginResult.RefreshToken}");
            sb.AppendLine();
            Account.Token = loginResult.IdentityToken;
            sb.AppendLine("Claims");
            sb.AppendLine("------");
            
            foreach (var claim in loginResult.User.Claims)
            {
                if (claim.Type == "name"){
                    Account.Name = claim.Value;
                }
                else if (claim.Type == "email"){
                    Account.Email = claim.Value;
                }
                else if (claim.Type == "picture"){
                    Account.Picture = claim.Value;
                }
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }
            

            resultTextBox.Text = sb.ToString();
            Authenticate(null, null);
            Close();
            MainWindow mainWindow = new MainWindow(restClient);
            mainWindow.ShowDialog();
        }
        
        private void Authenticate(object sender, RoutedEventArgs e)
        {
            
            
            var request = new RestRequest("api/client_login", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(Account.Token));
            var response = restClient.Execute(request);
            restClient.CookieContainer = new System.Net.CookieContainer();
            var authCookie = response.Cookies.First(a => a.Name == "auth_tkt");  
            restClient.CookieContainer.Add(new System.Net.Cookie(authCookie.Name, authCookie.Value, authCookie.Path, authCookie.Domain));
            Trace.WriteLine(response);
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionNameComboBox.ItemsSource = _connectionNames;
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            BrowserResultType browserResult = await client.LogoutAsync();

            if (browserResult != BrowserResultType.Success)
            {
                resultTextBox.Text = browserResult.ToString();
                return;
            }

            logoutButton.Visibility = Visibility.Collapsed;
            loginButton.Visibility = Visibility.Visible;

            audienceTextBox.Text = "";
            resultTextBox.Text = "";
            connectionNameComboBox.ItemsSource = _connectionNames;
        }
        
        
    }
}