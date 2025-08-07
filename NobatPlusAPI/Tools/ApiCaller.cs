using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusAPI.Tools
{
    public class ApiCaller
    {
        public async Task<T> Call<T>(string apiUrl,string apiMethod,string apiBody, List<ReqHeader> apiHeaders = null,Encoding encoding = null) where T : new()
        {
            string resultJson = "";
            T res = new T();
            if(apiHeaders == null) apiHeaders = new List<ReqHeader>();
            var client = new HttpClient();
            var request = new HttpRequestMessage(GetHttpMethod(apiMethod), apiUrl);
           foreach( ReqHeader header in apiHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            var content = new StringContent(apiBody, encoding, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            resultJson = await response.Content.ReadAsStringAsync();
            if (response != null)
            {
                res = JsonConvert.DeserializeObject<T>(resultJson);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                }


            }
            return res;
        }


        private HttpMethod GetHttpMethod(string method)
        {
            switch (method.ToLower())
            {
                default:
                case "get":
                    return HttpMethod.Get;
                case "post":
                    return HttpMethod.Post;
                case "put":
                    return HttpMethod.Put;
                case "patch":
                    return HttpMethod.Patch;
                case "delete":
                    return HttpMethod.Delete;
            }


        }
        public class ReqHeader
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
