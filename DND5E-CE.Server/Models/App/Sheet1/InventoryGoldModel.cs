using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class InventoryGoldModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; } = null!;

        [Required]
        // Copper pieces
        public int Cp { get; set; } = 0;

        [Required]
        // Silver pieces
        public int Sp { get; set; } = 0;

        [Required]
        // Electrum pieces
        public int Ep { get; set; } = 0;

        [Required]
        // Gold pieces
        public int Gp { get; set; } = 0;

        [Required]
        // Platinum pieces
        public int Pp { get; set; } = 0;
    }
}
