using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class ClassResourceModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; } = null!;

        [Required]
        public int Total { get; set; } = 0;

        [Required]
        public int Current {  get; set; } = 0;

        [Required]
        public string Name { get; set; } = "Ресуср класса";

        [Required]
        public bool IsUsePb { get; set; } = false;

        [Required]
        public string ResetOn { get; set; } = "longRest"; // "longRest" | "shortRest" | "-";
    }
}
