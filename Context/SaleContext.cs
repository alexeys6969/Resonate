using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Resonate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Resonate.Model.SaleClasses;

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
                        var salesData = JArray.Parse(jsonResponse);
                        var sales = new List<Sale>();

                        foreach (var saleJson in salesData)
                        {
                            var sale = new Sale
                            {
                                Id = GetIntValue(saleJson, "id"),
                                Code = GetStringValue(saleJson, "code"),
                                Employee_id = GetIntValue(saleJson, "employee_id"),
                                Sale_Date = GetDateTimeValue(saleJson, "sale_Date"),
                                Total_Amount = GetDecimalValue(saleJson, "total_Amount"),
                                Employee = new Employees
                                {
                                    Full_Name = GetStringValue(saleJson, "employee_Name"),
                                    Position = GetStringValue(saleJson, "employee_Position")
                                },
                                Sale_Items = new List<SaleItem>()
                            };

                            // Парсим товары (API возвращает "items" с маленькой буквы)
                            var itemsJson = saleJson["items"] ?? saleJson["Items"];
                            if (itemsJson != null)
                            {
                                foreach (var itemJson in itemsJson)
                                {
                                    sale.Sale_Items.Add(new SaleItem
                                    {
                                        Id = GetIntValue(itemJson, "id"),
                                        Product_id = GetIntValue(itemJson, "product_id"),
                                        Quantity = GetIntValue(itemJson, "quantity"),

                                        // 🔹 ИСПРАВЛЕНО: точное имя поля из API
                                        Price_At_Sale = GetDecimalValue(itemJson, "price_At_Sale"),

                                        Product = new Product
                                        {
                                            Name = GetStringValue(itemJson, "name")
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
                            Id = GetIntValue(saleJson, "id"),
                            Code = GetStringValue(saleJson, "code"),
                            Employee_id = GetIntValue(saleJson, "employee_id"),
                            Sale_Date = GetDateTimeValue(saleJson, "sale_Date"),
                            Total_Amount = GetDecimalValue(saleJson, "total_Amount"),
                            Employee = new Employees
                            {
                                Full_Name = GetStringValue(saleJson, "employee_Name"),
                                Position = GetStringValue(saleJson, "employee_Position")
                            },
                            Sale_Items = new List<SaleItem>()
                        };

                        // Парсим товары
                        var itemsJson = saleJson["items"] ?? saleJson["Items"];
                        if (itemsJson != null)
                        {
                            foreach (var itemJson in itemsJson)
                            {
                                sale.Sale_Items.Add(new SaleItem
                                {
                                    Id = GetIntValue(itemJson, "id"),
                                    Product_id = GetIntValue(itemJson, "product_id"),
                                    Quantity = GetIntValue(itemJson, "quantity"),

                                    // 🔹 ИСПРАВЛЕНО: точное имя поля из API
                                    Price_At_Sale = GetDecimalValue(itemJson, "price_At_Sale"),

                                    Product = new Product
                                    {
                                        Name = GetStringValue(itemJson, "name")
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
                using (HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, url + "POSTSale"))
                {
                    string json = JsonConvert.SerializeObject(request);
                    requestMsg.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(requestMsg);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = JObject.Parse(jsonResponse);
                        var saleJson = responseData["sale"] ?? responseData;

                        return new Sale
                        {
                            Id = GetIntValue(saleJson, "id"),
                            Code = GetStringValue(saleJson, "code"),
                            Sale_Date = GetDateTimeValue(saleJson, "sale_Date"),
                            Total_Amount = GetDecimalValue(saleJson, "total_Amount"),
                            Employee = new Employees
                            {
                                Id = GetIntValue(saleJson["employee"], "id"),
                                Full_Name = GetStringValue(saleJson["employee"], "full_Name")
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
                using (HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Put, url + $"PUTSale?id={id}"))
                {
                    string json = JsonConvert.SerializeObject(request);
                    requestMsg.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(requestMsg);
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

        // 🔹 Вспомогательные методы для парсинга с учётом регистра API
        private static int GetIntValue(JToken token, string propertyName)
        {
            if (token == null) return 0;

            // Пробуем найти свойство с разным регистром
            var value = token[propertyName] ??
                        token[propertyName.ToLower()] ??
                        token[propertyName.ToUpper()] ??
                        token.SelectToken(propertyName, true);

            return value?.Value<int>() ?? 0;
        }

        private static string GetStringValue(JToken token, string propertyName)
        {
            if (token == null) return string.Empty;

            var value = token[propertyName] ??
                        token[propertyName.ToLower()] ??
                        token[propertyName.ToUpper()];

            return value?.Value<string>() ?? string.Empty;
        }

        private static decimal GetDecimalValue(JToken token, string propertyName)
        {
            if (token == null) return 0;

            // 🔹 Для цены: проверяем все варианты, включая точный из вашего API
            var value = token[propertyName] ??                          // Price_At_Sale
                        token[propertyName.ToLower()] ??                // price_at_sale
                        token[char.ToLower(propertyName[0]) + propertyName.Substring(1)] ?? // price_At_Sale ← ВАШ ФОРМАТ!
                        token["price_At_Sale"] ??                       // явное указание
                        token["price_at_sale"] ??
                        token["price"] ??
                        token["Price"];

            return value?.Value<decimal>() ?? 0;
        }

        private static DateTime GetDateTimeValue(JToken token, string propertyName)
        {
            if (token == null) return DateTime.Now;

            var value = token[propertyName] ??
                        token[propertyName.ToLower()] ??
                        token[propertyName.ToUpper()];

            if (value != null && value.Type == JTokenType.String)
            {
                if (DateTime.TryParse(value.ToString(), out var result))
                    return result;
            }

            return value?.Value<DateTime>() ?? DateTime.Now;
        }
    }
}