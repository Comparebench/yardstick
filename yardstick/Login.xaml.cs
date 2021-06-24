using Auth0.OidcClient;
using IdentityModel.OidcClient;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace yardstick
{
    public partial class Login
    {
        private Auth0Client _client;
        private readonly RestClient _restClient = new RestClient("https://comparebench.com");

        public Login(){
            
            InitializeComponent();
            if (File.Exists("auth.json")){
                using (StreamReader file = File.OpenText("auth.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Account _account = (Account)serializer.Deserialize(file, typeof(Account));
                    Authenticate(_account);
            
                    MainWindow mainWindow = new MainWindow(_restClient, _account);
                    Close();
                    mainWindow.ShowDialog();
                }
                
            }
            else{
                DiscordLogin();
            }

        }

        private async void DiscordLogin(){
            var domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            var clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            _client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId
            });
            var extraParameters = new Dictionary<string, string>{{"connection", "discord"}};

            DisplayResult(await _client.LoginAsync(extraParameters: extraParameters));
        }
        
        private void DisplayResult(LoginResult loginResult){
            var _account = new Account();
            foreach (var claim in loginResult.User.Claims){
                switch (claim.Type){
                    case "name":
                        _account.Name = claim.Value;
                        break;
                    case "email":
                        _account.Email = claim.Value;
                        break;
                    case "picture":
                        _account.Picture = claim.Value;
                        break;
                }
            }
            _account.Token = loginResult.IdentityToken;
            
            if(!File.Exists("auth.json"))
                File.Create("auth.json").Close();
            var serializedAcc = JsonConvert.SerializeObject(_account);
            File.WriteAllText("auth.json", serializedAcc);
            

            Authenticate(_account);
            
            MainWindow mainWindow = new MainWindow(_restClient, _account);
            Close();
            mainWindow.ShowDialog();
        }
        
        private void Authenticate(Account account)
        {
            var request = new RestRequest("api/client_login", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(account.Token));
            var response = _restClient.Execute(request);
            _restClient.CookieContainer = new System.Net.CookieContainer();
            var authCookie = response.Cookies.First(a => a.Name == "auth_tkt");  
            _restClient.CookieContainer.Add(new System.Net.Cookie(authCookie.Name, authCookie.Value, authCookie.Path, authCookie.Domain));
            Trace.WriteLine(response);
        }
        
    }
}