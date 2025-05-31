using DND5E_CE.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheetController : ControllerBase
    {
        ILogger<SheetController> _logger;
        DND5EContext _context;
        public SheetController(ILogger<SheetController> logger, DND5EContext context)
        {
            _logger = logger;
            _context = context;
        }

        //[HttpPatch("{id}")]
        //public async Task<IActionResult> PatchSheet1(int id, [FromBody] )

    }
}
