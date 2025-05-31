using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class OtherResourceModel
    {
        [Key]
        public Guid Id { get; set; }

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; } = null!;

        [Required]
        public int Total { get; set; }

        [Required]
        public int Current { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsUsePb { get; set; }

        [Required]
        public string ResetOn { get; set; } // "longRest" | "shortRest" | "-";
    }
}
