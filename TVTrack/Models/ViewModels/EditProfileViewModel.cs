using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TVTrack.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [MaxLength(300)]
        public string? Bio { get; set; }

        public IFormFile? Avatar { get; set; }

        public string? CurrentAvatarUrl { get; set; }
    }
}
