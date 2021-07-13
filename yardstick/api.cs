using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Auth0.OidcClient;
using Newtonsoft.Json;
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

        public void UploadFiles(string[] filenames){
            
            RestRequest restRequest = new RestRequest("/api/benchmarks/uploadtest");
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.Method = Method.POST;
            restRequest.AddHeader("Content-Type", "multipart/form-data");
            foreach (String file in filenames){
                restRequest.AddFile("3dmark_upload", file);
            }
            var response = _restClient.Execute(restRequest);
            Trace.WriteLine(response.Content);
            
        }
        public ObservableCollection<Profile> getProfiles(){
            var request = new RestRequest("api/benchmarks/results", Method.POST);
            var response = _restClient.Execute(request);
            dynamic test = JObject.Parse(response.Content);
            ObservableCollection<Profile> Profiles = new ObservableCollection<Profile>();
            for (var i = 0; i < test.result.Count; i++){
                Profiles.Add(new Profile(test.result[i]));
            }
            return Profiles;
        }

        public void UploadResult(CurrentProfile currentProfile){
            var request = new RestRequest("api/benchmarks/upload", Method.POST);
            
            JsonSerializerSettings sets = new JsonSerializerSettings(){
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            String profileJson = JsonConvert.SerializeObject(currentProfile);

            /*if(!File.Exists(currentProfile.Name + ".json"))
                File.Create(currentProfile.Name + ".json").Close();
            File.WriteAllText(currentProfile.Name + ".json", irr);*/
            request.AddHeader("Content-Type", "multipart/form-data");
            foreach (String file in currentProfile.BenchmarkFiles){
                request.AddFile("3dmark_upload", file);
            }
            request.AddJsonBody(profileJson);
            var response = _restClient.Execute(request);
            dynamic responseContent = JObject.Parse(response.Content);
            var profile = new Profile(responseContent.profile);
            
            Trace.WriteLine(profile);
        }
    }
}