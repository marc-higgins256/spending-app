using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpendingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProtectedController : ControllerBase
    {
        [Authorize]
        [HttpGet("secret")]
        public IActionResult GetSecret()
        {
            return Ok(new { message = "This is a protected secret! You are authenticated." });
        }
    }
}
