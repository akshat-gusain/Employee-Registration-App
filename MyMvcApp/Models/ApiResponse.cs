namespace MyMvcApp.Models
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public List<RegisterViewModel> ModifiedID { get; set; }
    }

}
