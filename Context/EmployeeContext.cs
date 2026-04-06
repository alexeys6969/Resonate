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
        public static async Task<List<Employees>> GetEmployees(string token)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + $"GETEmployees?token={token}"))
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
        public static async Task<Employees> GetEmployeeById(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
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
        public static async Task<Employees> CreateEmployee(Employees employee)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "POSTEmployee"))
                {
                    Dictionary<string, string> FormData = new Dictionary<string, string>
                    {
                        ["Full_Name"] = employee.Full_Name,
                        ["Login"] = employee.Login,
                        ["Password"] = employee.Password,
                        ["Position"] = employee.Position
                    };

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(FormData);
                    Request.Content = Content;

                    var Response = await Client.SendAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.Created ||
                        Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        dynamic responseObj = JsonConvert.DeserializeObject(sResponse);
                        return new Employees
                        {
                            Full_Name = responseObj.Full_Name,
                            Login = responseObj.Login,
                            Password = responseObj.Password,
                            Position = responseObj.Position
                        };
                    }
                    else
                    {
                        string error = await Response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка при создании сотрудника: {error}");
                    }
                }
            }
        }
        public static async Task<bool> UpdateEmployee(int id, Employees employee)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url + "PUTEmployee"))
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(id.ToString()), "id");
                    content.Add(new StringContent(employee.Full_Name ?? ""), "Full_Name");
                    content.Add(new StringContent(employee.Login ?? ""), "Login");
                    content.Add(new StringContent(employee.Password ?? ""), "Password");
                    content.Add(new StringContent(employee.Position ?? ""), "Position");

                    request.Content = content;

                    var response = await client.SendAsync(request);

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseBody}");

                    return response.IsSuccessStatusCode;
                }
            }
        }
        public static async Task<bool> DeleteEmployee(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "DELETEEmployees"))
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

