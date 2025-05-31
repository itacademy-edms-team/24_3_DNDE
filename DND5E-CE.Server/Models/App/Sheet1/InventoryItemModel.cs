using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class InventoryItemModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; } = null!;

        [Required]
        public int Amount { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

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
        public string Prop { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
