namespace MyMvcApp.Models
{
    public class Response
    {
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public object modifiedID { get; set; } // Adjust type if necessary
    }
}
