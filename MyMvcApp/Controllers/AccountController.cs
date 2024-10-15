using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Services;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using LogRegApi.Models;
using System.Text.Json;

namespace MyMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiService _apiService;

        // Constructor that uses dependency injection
        public AccountController(HttpClient httpClient, ApiService apiService)
        {
            _httpClient = httpClient;
            _apiService = apiService;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _apiService.RegisterAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Registration failed.");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            TempData.Remove("Username");
            if (ModelState.IsValid)
            {
                // Call the API to log in
                var (result, message) = await _apiService.LoginAsync(model);

                if (result) // This means API returned success
                {
                    TempData["Username"] = model.EmpEmail; // Store username for display
                    return RedirectToAction("Dashboard");
                }
                else // Handle API response failure
                {
                    ModelState.AddModelError("", message); // Use message to provide feedback
                                                           // Log the invalid login attempt
                }
            }
            return View(model);
        }


        public async Task<ActionResult> Dashboard()
        {
            var username = TempData["Username"]?.ToString();
            ViewBag.Username = username;

            var response = await _apiService.GetUsersAsync();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Dashboard response: {response.StatusCode} - {content}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                // Deserialize into the ApiResponse class
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(data);

                // Check if ModifiedID contains any users
                if (apiResponse != null && apiResponse.ModifiedID != null && apiResponse.ModifiedID.Count > 0)
                {
                    return View(apiResponse.ModifiedID); // Pass the list of users to the view
                }
            }

            ViewBag.ErrorMessage = "Failed to load user data.";
            return View(new List<RegisterViewModel>()); // Return an empty list if there was an error
        }



        public async Task<IActionResult> Edit(int id)
        {
            // Call the GET API to get the employee details
            var response = await _httpClient.GetFromJsonAsync<LogRegApi.Models.Response>($"https://localhost:7039/api/Registration/GetEmployeeById/{id}");

            // Check if the employee data is retrieved successfully
            if (response != null && response.statusCode == 200 && response.modifiedID != null)
            {
                // Cast the modifiedID to Registration
                var registration = System.Text.Json.JsonSerializer.Deserialize<Registration>(System.Text.Json.JsonSerializer.Serialize(response.modifiedID));

                // Map to RegisterViewModel
                var model = new RegisterViewModel
                {
                    EmpId = registration.EmpID,
                    EmpName = registration.EmpName,
                    EmpEmail = registration.EmpEmail,
                    // Add any other properties as necessary
                };

                return View(model); // Pass the mapped RegisterViewModel to the view
            }
            else
            {
                return NotFound(); // Handle not found case
            }
        }




        [HttpPost]
        public async Task<IActionResult> Edit(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Call the PUT API to update the employee details
                var response = await _httpClient.PutAsJsonAsync($"https://localhost:7039/api/Registration/UpdateRegistration/{model.EmpId}", model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update employee details.");
                }
            }

            return View(model); // Redisplay the form with the current data in case of failure
        }





        [HttpDelete]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            try
            {
                var result = await _apiService.DeleteUserAsync(id); // Use the updated method to delete by ID

                if (result)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete user." });
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
