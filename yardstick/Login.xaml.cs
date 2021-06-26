using System;
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
    {public Login(){
            InitializeComponent();
        }
        private async void DiscordLogin(object sender, EventArgs e){
            var domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            var clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            var _client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId
            });
            var extraParameters = new Dictionary<string, string>{{"connection", "discord"}};

            processResult(await _client.LoginAsync(extraParameters: extraParameters));
            
        }

        private void processResult(LoginResult loginResult){
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
            
            if(!File.Exists("auth.json"))
                File.Create("auth.json").Close();
            var serializedAcc = JsonConvert.SerializeObject(Account.Token);
            File.WriteAllText("auth.json", serializedAcc);
            Close();

        }
        
    }
}