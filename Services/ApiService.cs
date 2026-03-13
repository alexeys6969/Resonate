using Resonate.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft;

namespace Resonate.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService()
        {
            // Замените на URL вашего API
            _baseUrl = "https://localhost:44338/";

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Если уже есть сохраненный токен, добавляем его
            if (TokenStorage.Instance.IsAuthenticated)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStorage.Instance.Token);
            }
        }

        // Метод для входа
        public async Task<LoginResponse> LoginAsync(string login, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Login = login,
                    Password = password
                };

                // Сериализуем в JSON
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/EmployeesController/SingIn", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponse>(responseJson);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка входа: {error}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Универсальный метод для GET запросов
        public async Task<T> GetAsync<T>(string endpoint)
        {
            if (!await EnsureAuthenticated())
                return default(T);

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запроса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return default(T);
            }
        }

        // Универсальный метод для POST запросов
        public async Task<T> PostAsync<T, TData>(string endpoint, TData data)
        {
            if (!await EnsureAuthenticated())
                return default(T);

            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запроса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return default(T);
            }
        }

        // Проверка авторизации
        private async Task<bool> EnsureAuthenticated()
        {
            if (!TokenStorage.Instance.IsAuthenticated)
            {
                MessageBox.Show("Необходимо авторизоваться", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Обновляем токен в заголовке
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TokenStorage.Instance.Token);

            return true;
        }

        // Обработка ответа
        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                MessageBox.Show("Сессия истекла. Войдите снова.", "Ошибка авторизации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TokenStorage.Instance.ClearToken();
                return default(T);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Ошибка: {error}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return default(T);
            }
        }
    }
}
