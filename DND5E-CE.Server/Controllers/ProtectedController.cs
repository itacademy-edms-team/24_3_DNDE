using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("protected-data")]
        public IActionResult GetProtectedData()
        {
            return Ok(new { Message = "This is protected data", User = User.Identity.Name});
        }
    }
}
