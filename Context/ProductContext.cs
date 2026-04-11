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
    public class ProductContext
    {
        static string url = @"https://localhost:7133/";
        public static async Task<List<Product>> GetProducts()
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "GETProducts"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        List<Product> products = JsonConvert.DeserializeObject<List<Product>>(sResponse);
                        return products;
                    }
                }
            }
            return new List<Product>();
        }
        public static async Task<Product> GetProductById(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + $"GETProductById?id={id}"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Product Product = JsonConvert.DeserializeObject<Product>(sResponse);
                        return Product;
                    }
                }
            }
            return null;
        }
        public static async Task<Product> CreateProduct(Product product)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url + "POSTProduct"))
                {
                    var formData = new Dictionary<string, string>
                    {
                        ["Article"] = product.Article,
                        ["Name"] = product.Name,
                        ["Category_Id"] = product.Category_Id.ToString(),
                        ["Price"] = product.Price.ToString(),
                        ["Stock_Quantity"] = product.Stock_Quantity.ToString(),
                        ["Description"] = product.Description
                    };

                    request.Content = new FormUrlEncodedContent(formData);
                    var response = await client.SendAsync(request);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var json = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);

                        return new Product
                        {
                            Id = (int)(json["id"] ?? json["Id"] ?? 0),
                            Article = (string)(json["article"] ?? json["Article"] ?? ""),
                            Name = (string)(json["name"] ?? json["Name"] ?? ""),
                            Description = (string)(json["description"] ?? json["Description"] ?? ""),
                            Category_Id = (int)(json["category_Id"] ?? json["Category_Id"] ?? 0),
                            Price = (decimal)(json["price"] ?? json["Price"] ?? 0),
                            Stock_Quantity = (int)(json["stock_Quantity"] ?? json["Stock_Quantity"] ?? 0)
                        };
                    }
                    else
                    {
                        throw new Exception($"Ошибка сервера: {jsonResponse}");
                    }
                }
            }
        }
        public static async Task<bool> UpdateProduct(int id, Product product)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url + "PUTProduct"))
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(id.ToString()), "id");
                    content.Add(new StringContent(product.Name ?? ""), "Name");
                    content.Add(new StringContent(product.Description ?? ""), "Description");
                    content.Add(new StringContent(product.Article ?? ""), "Article");
                    content.Add(new StringContent(product.Category_Id.ToString()), "Category_Id");
                    content.Add(new StringContent(product.Price.ToString()), "Price");
                    content.Add(new StringContent(product.Stock_Quantity.ToString()), "Stock");


                    request.Content = content;

                    var response = await client.SendAsync(request);

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseBody}");

                    return response.IsSuccessStatusCode;
                }
            }
        }
        public static async Task<bool> DeleteProduct(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "DELETEProducts"))
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
