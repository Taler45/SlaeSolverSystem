using SlaeSolver.BLL.DTOs.Tasks;

namespace SlaeSolver.BLL.Services.Tasks;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(Guid userId, CreateTaskRequest request);
    Task<TaskDto> GetAsync(Guid userId,Guid taskId);
    Task<IEnumerable<TaskDto>> GetHistoryAsync(Guid userId,int page, int pageSize);
    Task<TaskDto> CancelAsync(Guid userId,Guid taskId);
}