using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class CharacterCreateDto
    {
        [Required]
        public Sheet1CreateDto Sheet1 { get; set; }
        public Sheet2CreateDto? Sheet2 { get; set; }
        public Sheet3CreateDto? Sheet3 { get; set; }
    }
}
