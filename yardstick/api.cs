using RestSharp;

namespace yardstick{
    public class Api{
        
        public TResponse Post<TResponse>(string relativeUri, object postBody) where TResponse : new()
        {
            //Note: Ideally the RestClient isn't created for each request. 
            var restClient = new RestClient("https://comparebench.com");

            var restRequest = new RestRequest(relativeUri, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            restRequest.AddJsonBody(postBody);

            var result = restClient.Post<TResponse>(restRequest);

            return result.Data;
        }
    }
}