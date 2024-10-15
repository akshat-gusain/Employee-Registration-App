using System.Text.Json.Serialization;

namespace LogRegApi.Models
{
    public class Registration
    {
        [JsonPropertyName("empID")]
        public int EmpID { get; set; }

        [JsonPropertyName("empName")]
        public string EmpName { get; set; }

        [JsonPropertyName("empPassword")]
        public string EmpPassword { get; set; }

        [JsonPropertyName("empEmail")]
        public string EmpEmail { get; set; }
    }
}
