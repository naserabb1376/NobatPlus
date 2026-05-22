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
        public async Task<T> Call<T>(
    string apiUrl,
    string apiMethod,
    string apiBody,
    List<ReqHeader> apiHeaders = null,
    Encoding encoding = null,
    string contentType = "application/json"  // ✅ پارامتر جدید برای تعیین نوع Content-Type
) where T : new()
        {
            string resultJson = "";
            T res = new T();
            if (apiHeaders == null) apiHeaders = new List<ReqHeader>();
            if (encoding == null) encoding = Encoding.UTF8;

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(GetHttpMethod(apiMethod), apiUrl);

                // ✅ اضافه کردن Headerها
                foreach (ReqHeader header in apiHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // ✅ تنظیم Body بر اساس contentType
                if (!string.IsNullOrEmpty(apiBody))
                {
                    if (contentType == "application/x-www-form-urlencoded")
                    {
                        request.Content = new StringContent(apiBody, encoding, "application/x-www-form-urlencoded");
                    }
                    else if (contentType == "application/json")
                    {
                        request.Content = new StringContent(apiBody, encoding, "application/json");
                    }
                    else
                    {
                        // fallback برای هر نوع دیگر
                        request.Content = new StringContent(apiBody, encoding, contentType);
                    }
                }

                var response = await client.SendAsync(request);
                resultJson = await response.Content.ReadAsStringAsync();

                if (response != null)
                {
                    try
                    {
                        res = JsonConvert.DeserializeObject<T>(resultJson);
                    }
                    catch
                    {
                        // اگر پاسخ JSON نبود، res رو همونطور خالی می‌ذاره
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }
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
