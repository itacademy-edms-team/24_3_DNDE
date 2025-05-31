using DND5E_CE.Server.DTO.App.Sheet1;
using DND5E_CE.Server.DTO.App.Sheet2;
using DND5E_CE.Server.DTO.App.Sheet3;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class CharacterCreateDto
    {
        [Required(ErrorMessage = "Sheet1 is required")]
        public Sheet1CreateDto Sheet1 { get; set; }
    }
}
