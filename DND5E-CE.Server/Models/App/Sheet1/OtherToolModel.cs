using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class OtherToolModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public string Name { get; set; } = "Новое владение";

        [Required]
        public string Type { get; set; } = "Language"; // "Language" | "Weapon" | "Armor" | "Other";
    }
}
