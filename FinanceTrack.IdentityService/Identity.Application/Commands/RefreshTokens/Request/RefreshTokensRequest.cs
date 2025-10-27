using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands.RefreshTokens.Request
{
    public class RefreshTokensRequest : Commands.Request
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
