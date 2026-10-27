using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlaeSolver.API.Infrastructure;
using SlaeSolver.BLL.DTOs.Tasks;
using SlaeSolver.BLL.Services.Tasks;

namespace SlaeSolver.API.Controllers;


[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController: ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("create/task")]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request)
    {
        var dto = await _taskService.CreateAsync(User.GetUserId(), request);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _taskService.GetHistoryAsync(User.GetUserId(), page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Get([FromRoute] Guid id)
    {
        return Ok(await _taskService.GetAsync(User.GetUserId(), id));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<TaskDto>> Cancel([FromRoute] Guid id)
    {
        return Ok(await _taskService.CancelAsync(User.GetUserId(), id));
    }
    
}