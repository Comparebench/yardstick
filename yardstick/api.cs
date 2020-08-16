using RestSharp;

namespace yardstick{
    public class API{
        
        public TResponse Post<TResponse>(string relativeUri, object postBody) where TResponse : new()
        {
            //Note: Ideally the RestClient isn't created for each request. 
            var restClient = new RestClient("http://localhost:6543");

            var restRequest = new RestRequest(relativeUri, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            restRequest.AddBody(postBody);

            var result = restClient.Post<TResponse>(restRequest);

            return result.Data;
        }
    }
}