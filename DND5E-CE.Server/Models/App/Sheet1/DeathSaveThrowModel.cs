using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class DeathSaveThrowModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public int SuccessTotal { get; set; } = 0;
        
        [Required]
        public int FailuresTotal { get; set; } = 0;
    }
}
