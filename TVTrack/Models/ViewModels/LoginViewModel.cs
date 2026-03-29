using System.ComponentModel.DataAnnotations;

namespace TVTrack.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
