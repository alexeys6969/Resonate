using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Resonate.Model.SaleClasses;
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
    public class SaleContext
    {
        static string url = @"https://localhost:7133/";

        /// <summary>
        /// Получить все продажи
        /// </summary>
        public static async Task<List<Sale>> GetSales()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url + "GETSales"))
                {
                    var response = await client.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Парсим сложный ответ с вложенными товарами
                        var salesData = JArray.Parse(jsonResponse);
                        var sales = new List<Sale>();

                        foreach (var saleJson in salesData)
                        {
                            var sale = new Sale
                            {
                                Id = (int)(saleJson["Id"] ?? saleJson["id"] ?? 0),
                                Code = (string)(saleJson["Code"] ?? saleJson["code"] ?? ""),
                                Employee_id = (int)(saleJson["Employee_id"] ?? saleJson["employee_id"] ?? 0),
                                Sale_Date = DateTime.Parse((string)(saleJson["Sale_Date"] ?? saleJson["sale_date"] ?? DateTime.Now.ToString())),
                                Total_Amount = (decimal)(saleJson["Total_Amount"] ?? saleJson["total_amount"] ?? 0),
                                Employee = new Employees
                                {
                                    Full_Name = (string)(saleJson["Employee_Name"] ?? ""),
                                    Position = (string)(saleJson["Employee_Position"] ?? "")
                                },
                                Sale_Items = new List<SaleItem>()
                            };

                            // Парсим товары продажи
                            if (saleJson["Items"] != null)
                            {
                                foreach (var itemJson in saleJson["Items"])
                                {
                                    sale.Sale_Items.Add(new SaleItem
                                    {
                                        Id = (int)(itemJson["Id"] ?? itemJson["id"] ?? 0),
                                        Product_id = (int)(itemJson["Product_id"] ?? itemJson["product_id"] ?? 0),
                                        Quantity = (int)(itemJson["Quantity"] ?? itemJson["quantity"] ?? 0),
                                        Price_At_Sale = (decimal)(itemJson["Price_At_Sale"] ?? itemJson["price_at_sale"] ?? 0),
                                        Product = new Product
                                        {
                                            Name = (string)(itemJson["Name"] ?? itemJson["name"] ?? "")
                                        }
                                    });
                                }
                            }

                            sales.Add(sale);
                        }

                        return sales;
                    }
                }
            }
            return new List<Sale>();
        }

        /// <summary>
        /// Получить продажу по ID
        /// </summary>
        public static async Task<Sale> GetSaleById(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url + $"GETSaleById?id={id}"))
                {
                    var response = await client.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var saleJson = JObject.Parse(jsonResponse);

                        var sale = new Sale
                        {
                            Id = (int)(saleJson["Id"] ?? saleJson["id"] ?? 0),
                            Code = (string)(saleJson["Code"] ?? saleJson["code"] ?? ""),
                            Employee_id = (int)(saleJson["Employee_id"] ?? saleJson["employee_id"] ?? 0),
                            Sale_Date = DateTime.Parse((string)(saleJson["Sale_Date"] ?? saleJson["sale_date"] ?? DateTime.Now.ToString())),
                            Total_Amount = (decimal)(saleJson["Total_Amount"] ?? saleJson["total_amount"] ?? 0),
                            Employee = new Employees
                            {
                                Full_Name = (string)(saleJson["Employee_Name"] ?? ""),
                                Position = (string)(saleJson["Employee_Position"] ?? "")
                            },
                            Sale_Items = new List<SaleItem>()
                        };

                        // Парсим товары
                        if (saleJson["Items"] != null)
                        {
                            foreach (var itemJson in saleJson["Items"])
                            {
                                sale.Sale_Items.Add(new SaleItem
                                {
                                    Id = (int)(itemJson["Id"] ?? itemJson["id"] ?? 0),
                                    Product_id = (int)(itemJson["Product_id"] ?? itemJson["product_id"] ?? 0),
                                    Quantity = (int)(itemJson["Quantity"] ?? itemJson["quantity"] ?? 0),
                                    Price_At_Sale = (decimal)(itemJson["Price_At_Sale"] ?? itemJson["price_at_sale"] ?? 0),
                                    Product = new Product
                                    {
                                        Name = (string)(itemJson["Name"] ?? itemJson["name"] ?? "")
                                    }
                                });
                            }
                        }

                        return sale;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Создать новую продажу
        /// </summary>
        public static async Task<Sale> CreateSale(CreateSaleRequest request)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request_msg = new HttpRequestMessage(HttpMethod.Post, url + "POSTSale"))
                {
                    // Сериализуем в JSON
                    string json = JsonConvert.SerializeObject(request);
                    request_msg.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request_msg);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = JObject.Parse(jsonResponse);

                        // Ответ приходит в формате { "sale": {...} }
                        var saleJson = responseData["sale"] ?? responseData;

                        return new Sale
                        {
                            Id = (int)(saleJson["Id"] ?? saleJson["id"] ?? 0),
                            Code = (string)(saleJson["Code"] ?? saleJson["code"] ?? ""),
                            Sale_Date = DateTime.Parse((string)(saleJson["Sale_Date"] ?? saleJson["sale_date"] ?? DateTime.Now.ToString())),
                            Total_Amount = (decimal)(saleJson["Total_Amount"] ?? saleJson["total_amount"] ?? 0),
                            Employee = new Employees
                            {
                                Id = (int)(saleJson["Employee"]?["Id"] ?? saleJson["Employee"]?["id"] ?? 0),
                                Full_Name = (string)(saleJson["Employee"]?["Full_Name"] ?? saleJson["Employee"]?["full_name"] ?? "")
                            },
                            Sale_Items = new List<SaleItem>()
                        };
                    }
                    else
                    {
                        throw new Exception($"Ошибка при создании продажи: {jsonResponse}");
                    }
                }
            }
        }

        /// <summary>
        /// Обновить продажу
        /// </summary>
        public static async Task<bool> UpdateSale(int id, UpdateSaleFullRequest request)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request_msg = new HttpRequestMessage(HttpMethod.Put, url + $"PUTSale?id={id}"))
                {
                    string json = JsonConvert.SerializeObject(request);
                    request_msg.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request_msg);

                    return response.IsSuccessStatusCode;
                }
            }
        }

        /// <summary>
        /// Удалить продажу
        /// </summary>
        public static async Task<bool> DeleteSale(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url + $"DELETESale?id={id}"))
                {
                    var response = await client.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
        }
    }
}
