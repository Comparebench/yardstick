﻿using System.Windows;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace yardstick
{
    public partial class Login
    {
        private Auth0Client _client;

        private readonly RestClient _restClient = new RestClient("https://comparebench.com");

        // private readonly string[] _connectionNames = new string[]{
        //     "google-oauth2",
        //     "discord",
        // };

        public Login(){
            InitializeComponent();
            DiscordLogin();
        }

        private async void DiscordLogin(){
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            _client = new Auth0Client(new Auth0ClientOptions{
                Domain = domain,
                ClientId = clientId
            });

            var extraParameters = new Dictionary<string, string>();
            extraParameters.Add("connection", "discord");
            DisplayResult(await _client.LoginAsync(extraParameters));
        }

        private void DisplayResult(LoginResult loginResult){
            foreach (var claim in loginResult.User.Claims){
                switch (claim.Type){
                    case "name":
                        Account.Name = claim.Value;
                        break;
                    case "email":
                        Account.Email = claim.Value;
                        break;
                    case "picture":
                        Account.Picture = claim.Value;
                        break;
                }
            }

            Account.Token = loginResult.IdentityToken;
            
            Authenticate(null, null);

            MainWindow mainWindow = new MainWindow(_restClient);
            Close();
            mainWindow.ShowDialog();
        }

        private void Authenticate(object sender, RoutedEventArgs e){
            var request = new RestRequest("api/client_login", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(Account.Token));
            var response = _restClient.Execute(request);
            _restClient.CookieContainer = new System.Net.CookieContainer();
            var authCookie = response.Cookies.First(a => a.Name == "auth_tkt");
            _restClient.CookieContainer.Add(new System.Net.Cookie(authCookie.Name, authCookie.Value, authCookie.Path,
                authCookie.Domain));
            Trace.WriteLine(response);
        }
    }
}