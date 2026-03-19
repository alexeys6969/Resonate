using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Resonate.Model;
using Resonate.Windows;

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
        public static async Task<List<Employees>> GetEmployees()
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "GETEmployees"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        List<Employees> Employees = JsonConvert.DeserializeObject<List<Employees>>(sResponse);
                        return Employees;
                    }
                }
            }
            return new List<Employees>();
        }
        public static async Task<Employees> GetEmployeeById(int id, string token = null)
        {
            using (HttpClient Client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                    Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + $"GETEmployeeById?id={id}"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Employees Employee = JsonConvert.DeserializeObject<Employees>(sResponse);
                        return Employee;
                    }
                }
            }
            return null;
        }
        public static async Task<Employees> GetCurrentEmployee(string token)
        {
            using (HttpClient Client = new HttpClient())
            {
                Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "GETCurrentEmployee"))
                {
                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Employees employee = JsonConvert.DeserializeObject<Employees>(sResponse);
                        return employee;
                    }
                    else if (Response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        InfoWindow info = new InfoWindow("Сессия истекла. Пожалуйста, войдите снова.");
                        info.Show();
                        return null;
                    }
                    else
                    {
                        string error = await Response.Content.ReadAsStringAsync();
                        InfoWindow info = new InfoWindow($"Ошибка: {error}");
                        info.Show();
                        return null;
                    }
                }
            }
        }
    }
} 

