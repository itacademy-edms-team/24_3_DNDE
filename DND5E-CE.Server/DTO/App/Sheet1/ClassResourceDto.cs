using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class ClassResourceDto
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Total { get; set; }

        [Required]
        public int Current { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsUsePb { get; set; }

        [Required]
        public string ResetOn { get; set; } // "longRest" | "shortRest" | "-";
    }
}
