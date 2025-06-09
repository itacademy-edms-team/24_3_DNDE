using DND5E_CE.Server.Models.App.Sheet1;
using DND5E_CE.Server.Models.App.Sheet3;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class CharacterModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        [Required]
        public Guid Sheet1Id { get; set; }

        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public Guid Sheet2Id { get; set; }

        public Sheet2Model Sheet2 { get; set; }

        [Required]
        public Guid Sheet3Id { get; set; }

        public Sheet3Model Sheet3 { get; set; }
    }
}
