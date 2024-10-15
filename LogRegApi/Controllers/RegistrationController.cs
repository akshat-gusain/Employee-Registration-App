using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using LogRegApi.Models;
using Microsoft.Extensions.Logging;
namespace LogRegApi.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(IConfiguration configuration, ILogger<RegistrationController> logger) // Inject the logger
        {
            _configuration = configuration;
            _logger = logger; // Initialize the logger
        }

        [HttpPost("InsertRegistration")]
        public Response InsertRegistration(Registration registration)
        {
            Response response = new Response();

            // Connection string from appsettings.json
            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spInsertRegistration", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add parameters
                cmd.Parameters.AddWithValue("@EmpName", registration.EmpName);
                cmd.Parameters.AddWithValue("@EmpPassword", registration.EmpPassword);
                cmd.Parameters.AddWithValue("@EmpEmail", registration.EmpEmail);

                conn.Open();
                int newID = Convert.ToInt32(cmd.ExecuteScalar()); // Get new ID

                if (newID > 0)
                {
                    response.statusCode = 200;
                    response.statusMessage = "Registration added successfully.";
                    response.modifiedID = newID;
                }
                else
                {
                    response.statusCode = 500;
                    response.statusMessage = "Error occurred while inserting registration.";
                    response.modifiedID = -1;
                }
            }

            return response;
        }

        [HttpGet("GetAllRegistrations")]
        public Response GetAllRegistrations()
        {
            Response response = new Response();
            List<Registration> registrations = new List<Registration>();

            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spGetAllRegistrations", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Registration registration = new Registration()
                    {
                        EmpID = Convert.ToInt32(reader["EmpID"]),
                        EmpName = reader["EmpName"].ToString(),
                        EmpPassword = reader["EmpPassword"].ToString(),
                        EmpEmail = reader["EmpEmail"].ToString()

                    };
                    registrations.Add(registration);
                }
            }

            response.statusCode = 200;
            response.statusMessage = "Data retrieved successfully.";
            response.modifiedID = registrations;
            return response;
        }

        [HttpPut("UpdateRegistration/{id}")]
        public IActionResult UpdateRegistration(int id, [FromBody] Registration registration)
        {
            Response response = new Response();
            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spUpdateRegistration", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@EmpID", id);
                cmd.Parameters.AddWithValue("@EmpName", registration.EmpName);
                cmd.Parameters.AddWithValue("@EmpPassword", registration.EmpPassword);
                cmd.Parameters.AddWithValue("@EmpEmail", registration.EmpEmail);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Return the updated registration object
                        response.statusCode = 200;
                        response.statusMessage = "Registration updated successfully.";
                        response.modifiedID = registration; // Return the updated registration object
                    }
                    else
                    {
                        response.statusCode = 404;
                        response.statusMessage = "Registration not found.";
                        response.modifiedID = null;
                    }
                }
                catch (Exception ex)
                {
                    response.statusCode = 500;
                    response.statusMessage = "An error occurred: " + ex.Message;
                    response.modifiedID = null;
                }
            }

            return Ok(response);
        }

        [HttpDelete("DeleteRegistration/{id}")]
        public IActionResult DeleteRegistration(int id)
        {
            Response response = new Response();
            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spDeleteRegistration", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@EmpID", id);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery(); // Execute the delete command

                    if (rowsAffected > 0)
                    {
                        response.statusCode = 200;
                        response.statusMessage = "Registration deleted successfully.";
                        response.modifiedID = id; // Optionally include a message or relevant data
                    }
                    else
                    {
                        response.statusCode = 404;
                        response.statusMessage = "Registration not found.";
                        response.modifiedID = -1;
                    }
                }
                catch (Exception ex)
                {
                    response.statusCode = 500;
                    response.statusMessage = "An error occurred: " + ex.Message;
                    response.modifiedID = -1;
                }
            }

            return Ok(response); // Return the response object
        }


        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            

            if (string.IsNullOrWhiteSpace(loginModel.EmpEmail) || string.IsNullOrWhiteSpace(loginModel.EmpPassword))
            {
                return BadRequest(new Response { /* Error details */ });
            }

            Response response = new Response();
            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Login", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@EmpEmail", SqlDbType.VarChar) { Value = loginModel.EmpEmail });
                cmd.Parameters.Add(new SqlParameter("@EmpPassword", SqlDbType.VarChar) { Value = loginModel.EmpPassword });

                try
                {
                    _logger.LogInformation("Attempting login with Email: {Email}", loginModel.EmpEmail);
                    conn.Open(); // Open the connection
                    SqlDataReader reader = cmd.ExecuteReader(); // Execute the command

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int empId = Convert.ToInt32(reader["EmpID"]);
                            response.statusCode = 200;
                            response.statusMessage = "Valid User";
                            response.modifiedID = empId; // Return EmpID in modifiedID
                        }
                    }
                    else
                    {
                        response.statusCode = 401; // Unauthorized
                        response.statusMessage = "Invalid User"; // Invalid credentials
                        response.modifiedID = null; // Set to null when invalid
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while logging in user: {Email}", loginModel.EmpEmail);
                    return StatusCode(500, new Response
                    {
                        statusCode = 500,
                        statusMessage = "Internal server error",
                        modifiedID = null
                    });
                }
            }

            return Ok(response); // Return the response
        }




        [HttpGet("GetEmployeeById/{id}")]
        public IActionResult GetEmployeeById(int id)
        {
            Response response = new Response();
            string connectionString = _configuration.GetConnectionString("RegCon");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spGetEmployeeById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@EmpID", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Registration registration = new Registration()
                        {
                            EmpID = Convert.ToInt32(reader["EmpID"]),
                            EmpName = reader["EmpName"].ToString(),
                            EmpPassword = reader["EmpPassword"].ToString(),
                            EmpEmail = reader["EmpEmail"].ToString()
                        };
                        response.modifiedID = registration;
                    }

                    response.statusCode = 200;
                    response.statusMessage = "Data retrieved successfully.";
                }
                else
                {
                    response.statusCode = 404;
                    response.statusMessage = "Employee not found.";
                }
            }

            return Ok(response);
        }



    }
}
