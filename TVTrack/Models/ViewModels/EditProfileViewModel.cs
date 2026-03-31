using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TVTrack.Models.ViewModels
{
    public class EditProfileViewModel
    {
        // Quick model class for editing user profile, has username, bio
        [MaxLength(300)]
        public string? Bio { get; set; }

        public IFormFile? Avatar { get; set; }

        public string? CurrentAvatarUrl { get; set; }
    }
}
