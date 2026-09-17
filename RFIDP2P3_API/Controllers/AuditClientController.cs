using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditClientController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditClientController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpPost("track")]
    public IActionResult TrackClientAction([FromBody] ClientAuditRequest request)
    {
        _auditService.Log(
            action: request.Action,
            entityName: request.Feature,
            payload: request.Metadata
        );

        return Ok(new { Message = "Action logged successfully" });
    }
}