using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class InventoryGoldDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        // Copper pieces
        public int Cp { get; set; }

        [Required]
        // Silver pieces
        public int Sp { get; set; }

        [Required]
        // Electrum pieces
        public int Ep { get; set; }

        [Required]
        // Gold pieces
        public int Gp { get; set; }

        [Required]
        // Platinum pieces
        public int Pp { get; set; }
    }
}
