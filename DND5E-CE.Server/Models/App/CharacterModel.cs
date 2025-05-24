using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class CharacterModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        [Required]
        public int Sheet1Id {  get; set; }

        [Required]
        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public int Sheet2Id { get; set; }

        [Required]
        [ForeignKey("Sheet2Id")]
        public Sheet2Model Sheet2 { get; set; }

        [Required]
        public int Sheet3Id { get; set; }

        [Required]
        [ForeignKey("Sheet3Id")]
        public Sheet3Model Sheet3 { get; set; }

    }
}
