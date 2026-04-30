using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Resonate.Model;

namespace Resonate.Context
{
    public class CategoryContext
    {
        static string url = @"https://localhost:7133/";

        private static string BuildUrl(string endpoint)
        {
            string token = Uri.EscapeDataString(MainWindow.Token ?? string.Empty);
            return url + endpoint + "?token=" + token;
        }

        public static async Task<List<Category>> GetCategories()
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "GETCategories"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        List<Category> Categories = JsonConvert.DeserializeObject<List<Category>>(sResponse);
                        return Categories;
                    }
                }
            }
            return new List<Category>();
        }
        public static async Task<Category> CreateCategory(Category category)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("POSTCategory")))
                {
                    Dictionary<string, string> FormData = new Dictionary<string, string>
                    {
                        ["Name"] = category.Name,
                        ["Description"] = category.Description
                    };

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(FormData);
                    Request.Content = Content;

                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.Created ||
                        Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        dynamic responseObj = JsonConvert.DeserializeObject(sResponse);
                        return new Category
                        {
                            Name = responseObj.Name,
                            Description = responseObj.Description
                        };
                    }
                    else
                    {
                        string error = await Response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка при создании категории: {error}");
                    }
                }
            }
        }
        public static async Task<bool> UpdateCategory(int id, Category category)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, BuildUrl("PUTCategory")))
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(id.ToString()), "id");
                    content.Add(new StringContent(category.Name ?? ""), "Name");
                    content.Add(new StringContent(category.Description ?? ""), "Description");

                    request.Content = content;

                    var response = await client.SendAsync(request);

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseBody}");

                    return response.IsSuccessStatusCode;
                }
            }
        }
        public static async Task<bool> DeleteCategory(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl("DELETECategory")))
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
