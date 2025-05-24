using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class Sheet1Model
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        // Header fields

        public string Name { get; set; } = "Новый персонаж";

        public string Class { get; set; } = String.Empty;

        public int Level { get; set; } = 1;

        public string Race { get; set; } = String.Empty;

        public string Backstory { get; set; } = String.Empty;

        public string Worldview { get; set; } = String.Empty;

        public string PlayerName { get; set; } = String.Empty;

        public int Experience { get; set; } = 0;

    }
}
