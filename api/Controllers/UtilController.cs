using Microsoft.AspNetCore.Mvc;
using shared.Models;

namespace api.Controllers;

[ApiController]
public class UtilController : ControllerBase
{
    [HttpGet("/system-health")]
    public StatusResponse GetHealth()
    {
        return new StatusResponse()
        {
            Status = "ok"
        };
    }
}
