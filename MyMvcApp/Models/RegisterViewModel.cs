using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models
{
    public class RegisterViewModel

    {
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public string EmpPassword { get; set; }
        public string EmpEmail { get; set; }
    }

}
