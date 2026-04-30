using Newtonsoft.Json;
using Resonate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Context
{
    public class SupplierContext
    {
        static string url = @"https://localhost:7133/";

        private static string BuildUrl(string endpoint)
        {
            string token = Uri.EscapeDataString(MainWindow.Token ?? string.Empty);
            return url + endpoint + "?token=" + token;
        }

        public static async Task<List<Supplier>> GetSuppliers()
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "GETSuppliers"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        List<Supplier> suppliers = JsonConvert.DeserializeObject<List<Supplier>>(sResponse);
                        return suppliers;
                    }
                }
            }
            return new List<Supplier>();
        }
        public static async Task<Supplier> CreateSupplier(Supplier supplier)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("POSTSupplier")))
                {
                    Dictionary<string, string> FormData = new Dictionary<string, string>
                    {
                        ["Name"] = supplier.Name,
                        ["Contact"] = supplier.Contact
                    };

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(FormData);
                    Request.Content = Content;

                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.Created ||
                        Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        dynamic responseObj = JsonConvert.DeserializeObject(sResponse);
                        return new Supplier
                        {
                            Name = responseObj.Name,
                            Contact = responseObj.Contact
                        };
                    }
                    else
                    {
                        string error = await Response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка при создании поставщика: {error}");
                    }
                }
            }
        }
        public static async Task<bool> UpdateSupplier(int id, Supplier supplier)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, BuildUrl("PUTSupplier")))
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(id.ToString()), "id");
                    content.Add(new StringContent(supplier.Name ?? ""), "Name");
                    content.Add(new StringContent(supplier.Contact ?? ""), "Contact");

                    request.Content = content;

                    var response = await client.SendAsync(request);

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseBody}");

                    return response.IsSuccessStatusCode;
                }
            }
        }
        public static async Task<bool> DeleteSupplier(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl("DELETESupplier")))
                {
                    Dictionary<string, string> FormData = new Dictionary<string, string>
                    {
                        ["id"] = id.ToString()
                    };

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(FormData);
                    Request.Content = Content;

                    var Response = await Client.SendAsync(Request);

                    return Response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
    }
}
