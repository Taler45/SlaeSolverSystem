namespace SlaeSolver.Domain.Entities;

public class UserEntitie
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } 
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    
    
    public ICollection<TaskEntitie> TaskEntities { get; set; } = new List<TaskEntitie>();
}
