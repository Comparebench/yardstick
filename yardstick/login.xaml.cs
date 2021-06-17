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
            DiscordLogin();

        }

        private async void DiscordLogin(){
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId
            });
            var extraParameters = new Dictionary<string, string>();
            extraParameters.Add("connection", "discord");
            DisplayResult(await client.LoginAsync(extraParameters: extraParameters));
        }
        
        private void DisplayResult(LoginResult loginResult){
            
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
            }
            Account.Token = loginResult.IdentityToken;
            

            Authenticate(null, null);
            
            MainWindow mainWindow = new MainWindow(restClient);
            Close();
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
        
    }
}