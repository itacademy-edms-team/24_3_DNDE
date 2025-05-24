using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class CharacterDto
    {
        public Sheet1Dto Sheet1 { get; set; }
        public Sheet2Dto Sheet2 { get; set; }
        public Sheet3Dto Sheet3 { get; set; }
    }
}
