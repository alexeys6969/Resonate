using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Resonate.Model;
using Resonate.Model.SupplyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Resonate.Context
{
    public class SupplyContext
    {
        private static readonly string url = @"https://localhost:7133/";

        public static async Task<List<Supply>> GetSupplies()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GETSupplies")))
                {
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                        return new List<Supply>();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var suppliesData = JArray.Parse(jsonResponse);
                    return suppliesData.Select(ParseSupply).Where(x => x != null).ToList();
                }
            }
        }

        public static async Task<Supply> GetSupplyById(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GETSupplyById", "id=" + id)))
                {
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                        return null;

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return ParseSupply(JObject.Parse(jsonResponse));
                }
            }
        }

        public static async Task<Supply> CreateSupply(CreateSupplyRequest request)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, BuildUrl("POSTSupply")))
                {
                    string json = JsonConvert.SerializeObject(request);
                    requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(requestMessage);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Ошибка при создании поставки: " + jsonResponse);

                    var responseData = JObject.Parse(jsonResponse);
                    return ParseSupply(responseData["supply"] ?? responseData);
                }
            }
        }

        public static async Task<bool> UpdateSupply(int id, UpdateSupplyFullRequest request)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, BuildUrl("PUTSupply", "id=" + id)))
                {
                    string json = JsonConvert.SerializeObject(request);
                    requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(requestMessage);
                    return response.IsSuccessStatusCode;
                }
            }
        }

        public static async Task<bool> DeleteSupply(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl("DELETESupply", "id=" + id)))
                {
                    var response = await client.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
        }

        private static string BuildUrl(string endpoint, string query = null)
        {
            string token = Uri.EscapeDataString(MainWindow.Token ?? string.Empty);
            if (string.IsNullOrWhiteSpace(query))
                return url + endpoint + "?token=" + token;

            return url + endpoint + "?token=" + token + "&" + query;
        }

        private static Supply ParseSupply(JToken supplyJson)
        {
            if (supplyJson == null)
                return null;

            JToken supplierToken = FindProperty(supplyJson, "supplier", "Supplier");
            int supplierId = GetIntValue(supplyJson, "supplier_id", "Supplier_id");
            if (supplierId == 0)
                supplierId = GetIntValue(supplierToken, "id", "Id");

            string supplierName = GetStringValue(supplyJson, "supplier_Name", "Supplier_Name");
            if (string.IsNullOrWhiteSpace(supplierName))
                supplierName = GetStringValue(supplierToken, "name", "Name");

            var supply = new Supply
            {
                Id = GetIntValue(supplyJson, "id", "Id"),
                Code = GetStringValue(supplyJson, "code", "Code"),
                Supplier_id = supplierId,
                Supply_Date = GetDateTimeValue(supplyJson, "supply_Date", "Supply_Date"),
                Total_Amount = GetDecimalValue(supplyJson, "total_Amount", "Total_Amount"),
                Supplier = new Supplier
                {
                    Id = supplierId,
                    Name = supplierName
                },
                Supply_Items = new List<SupplyItem>()
            };

            if (string.IsNullOrWhiteSpace(supply.Code) && supply.Id > 0)
                supply.Code = "SUPP-" + supply.Id;

            var itemsJson = FindProperty(supplyJson, "items", "Items") as JArray;
            if (itemsJson != null)
            {
                foreach (var itemJson in itemsJson)
                {
                    supply.Supply_Items.Add(new SupplyItem
                    {
                        Id = GetIntValue(itemJson, "id", "Id"),
                        Product_id = GetIntValue(itemJson, "product_id", "Product_id"),
                        Quantity = GetIntValue(itemJson, "quantity", "Quantity"),
                        Purchase_Price = GetDecimalValue(itemJson, "purchase_Price", "Purchase_Price", "price", "Price"),
                        Product = new Product
                        {
                            Id = GetIntValue(itemJson, "product_id", "Product_id"),
                            Name = GetStringValue(itemJson, "name", "Name", "productName", "ProductName")
                        }
                    });
                }
            }

            return supply;
        }

        private static JToken FindProperty(JToken token, params string[] propertyNames)
        {
            if (token == null || propertyNames == null)
                return null;

            foreach (string propertyName in propertyNames)
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                    continue;

                if (token[propertyName] != null)
                    return token[propertyName];

                foreach (var property in token.Children<JProperty>())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return property.Value;
                }
            }

            return null;
        }

        private static int GetIntValue(JToken token, params string[] propertyNames)
        {
            var value = FindProperty(token, propertyNames);
            return value != null ? value.Value<int>() : 0;
        }

        private static string GetStringValue(JToken token, params string[] propertyNames)
        {
            var value = FindProperty(token, propertyNames);
            return value != null ? value.Value<string>() ?? string.Empty : string.Empty;
        }

        private static decimal GetDecimalValue(JToken token, params string[] propertyNames)
        {
            var value = FindProperty(token, propertyNames);
            return value != null ? value.Value<decimal>() : 0m;
        }

        private static DateTime GetDateTimeValue(JToken token, params string[] propertyNames)
        {
            var value = FindProperty(token, propertyNames);
            if (value == null)
                return DateTime.Now;

            DateTime result;
            if (value.Type == JTokenType.String && DateTime.TryParse(value.ToString(), out result))
                return result;

            return value.Value<DateTime>();
        }
    }
}
