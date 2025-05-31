using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class HitPointModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public int Max { get; set; } = 3;

        [Required]
        public int Current { get; set; } = 3;

        [Required]
        public int Temp { get; set; } = 0;

    }
}
