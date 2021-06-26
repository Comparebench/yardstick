using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using DataFormat = RestSharp.DataFormat;

namespace yardstick{
    public class Api{
        private readonly RestClient _restClient = new RestClient("http://localhost:8180");
        private Auth0Client _auth0Client;
        public Api(){
            
        }
        public TResponse Post<TResponse>(string relativeUri, object postBody) where TResponse : new(){
            var restRequest = new RestRequest(relativeUri, Method.POST){
                RequestFormat = DataFormat.Json
            };
            restRequest.AddJsonBody(postBody);
            var result = _restClient.Post<TResponse>(restRequest);
            return result.Data;
        }

        public void Authenticate(bool retry=true){
            var request = new RestRequest("api/client_login", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(Account.Token));
            var response = _restClient.Execute(request);
            if (response.IsSuccessful == false && retry){
                var loginWindow = new Login();
                loginWindow.ShowDialog();
                Authenticate(false);
            }

            if (retry == false){
                // Failed init login and failed discord login
            }
            else{
                _restClient.CookieContainer = new System.Net.CookieContainer();
                var authCookie = response.Cookies.First(a => a.Name == "auth_tkt");
                _restClient.CookieContainer.Add(new System.Net.Cookie(authCookie.Name, authCookie.Value, authCookie.Path,
                    authCookie.Domain));
                Account.IsLoggedIn = true;
            }

        }

        public void getAccountDetails(){
            var request = new RestRequest("api/account/profile", Method.POST);
            var response = _restClient.Execute(request);
            dynamic test = JObject.Parse(response.Content);
            Account.DisplayName = test.user.display_name.ToString();
            
        }

        private void UploadResult(Profile profile){
            var request = new RestRequest("api/benchmarks/upload", Method.POST);

            /*BuildViewModel.Name = BuildName.Text;*/

            JsonSerializerSettings sets = new JsonSerializerSettings(){
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            String irr = JsonConvert.SerializeObject(profile);

            if(!File.Exists(profile.Name + ".json"))
                File.Create(profile.Name + ".json").Close();
            File.WriteAllText(profile.Name + ".json", irr);

            request.AddJsonBody(irr);
            var response = _restClient.Execute(request);
        }
    }
}