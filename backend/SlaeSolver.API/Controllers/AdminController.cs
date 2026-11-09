using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SlaeSolver.BLL.DTOs.Admin;
using SlaeSolver.BLL.Services.Admin;
using SlaeSolver.Domain;

namespace SlaeSolver.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin")]
public class AdminController: ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AdminOverviewDto>> GetOverview(CancellationToken ct)
    {
        var result = await _adminService.GetOverviewAsync(ct);
        return Ok(result);
    }
}