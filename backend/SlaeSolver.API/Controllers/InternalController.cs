using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlaeSolver.BLL.DTOs.Internal;
using SlaeSolver.BLL.Execution;
using SlaeSolver.BLL.Options;

namespace SlaeSolver.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("internal")]
public class InternalController: ControllerBase
{
    private readonly RunningTasksRegistry _registry;
    private readonly ServerOptions _options;

    public InternalController(RunningTasksRegistry registry, IOptions<ServerOptions> options)
    {
        _registry = registry;
        _options = options.Value;
    }

    [HttpGet("load")]
    public ActionResult<ServerLoadDto> GetLoad()
    {
        var providedKey = Request.Headers["X-Internal-Key"].ToString();
        var provider = Encoding.UTF8.GetBytes(providedKey);
        var optionsKey = Encoding.UTF8.GetBytes(_options.InternalApiKey);

        if (!CryptographicOperations.FixedTimeEquals(provider, optionsKey))
        {
            return Unauthorized();
        }

        var active = _registry.ActiveCount;
        return Ok(new ServerLoadDto(_options.ServerId,active, _options.MaxConcurrentTasks,
            _options.MaxConcurrentTasks - active, _registry.TotalWeight));
    }  
}