using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class Sheet1CreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public string Class { get; set; }
    }
}
