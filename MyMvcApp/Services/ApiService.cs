using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LogRegApi.Models;
using MyMvcApp.Models;
using Newtonsoft.Json;
namespace MyMvcApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/api/") // Use the correct base URL for API
            };
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Registration/InsertRegistration", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool IsSuccess, string Message)> LoginAsync(LoginModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7013/api/Registration/Login", content);

            if (response.IsSuccessStatusCode) // This checks for 200 status
            {
                return (true, "Login successful.");
            }
            else // Any other status
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }




        public async Task<HttpResponseMessage> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:7039/api/Registration/GetAllRegistrations");

            // Log the response content
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}"); // Use a logger or console to log the response

            return response;
        }



        public async Task<bool> UpdateUserAsync(RegisterViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Assuming the API uses EmpEmail or an Id to identify the user being updated
            var response = await _httpClient.PutAsync($"Registration/UpdateRegistration/{model.EmpEmail}", content);

            return response.IsSuccessStatusCode;
        }

        // Delete a specific user (DELETE API)
        public async Task<bool> DeleteUserAsync(int empId)
        {
            var response = await _httpClient.DeleteAsync($"Registration/DeleteRegistration/{empId}");

            // Log the response content for debugging
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Delete response: {response.StatusCode} - {content}");

            return response.IsSuccessStatusCode;
        }


    }
}

