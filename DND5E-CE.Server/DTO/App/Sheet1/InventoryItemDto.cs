using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class InventoryItemDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Weight { get; set; }

        [Required]
        public bool IsEquipped { get; set; }

        [Required]
        public bool IsUsedAsResource { get; set; }

        [Required]
        public bool IsHasAnAttack { get; set; }

        [Required]
        // Examples: "weapon", "armor", "consumable", "tool", ...
        public string Prop { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
