using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Resonate.Model;

namespace Resonate.Context
{
    public class EmployeeContext
    {
        static string url = @"https://localhost:7133/";
        public static async Task<string> Login(string login, string password)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "login"))
                {
                    Dictionary<string, string> FromData = new Dictionary<string, string>
                    {
                        ["login"] = login,
                        ["password"] = password
                    };

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(FromData);
                    Request.Content = Content;
                    var Response = await Client.SendAsync(Request);
                    if(Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Auth DataAuth = JsonConvert.DeserializeObject<Auth>(sResponse);
                        return DataAuth.Token;
                    }
                }
            }
            return null;
        }
    } 
}
