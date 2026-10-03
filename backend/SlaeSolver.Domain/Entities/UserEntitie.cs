namespace SlaeSolver.Domain.Entities;

public class UserEntitie
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } 
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public const int USER_NAME_LENGTH = 100;
    public const int EMAIL_AND_PASSWORD_LENGTH = 255;
    
    public ICollection<TaskEntitie> TaskEntities { get; set; } = new List<TaskEntitie>();


    private UserEntitie() { }

    private UserEntitie(Guid id, string firstName, string lastName, string email, string passwordHash)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
    }


    public static UserEntitie Create(string firstName, string lastName, string email, string passwordHash)
    {
        if (string.IsNullOrEmpty(firstName) || firstName.Length > USER_NAME_LENGTH)
        {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(lastName) || lastName.Length > USER_NAME_LENGTH)
        {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(email) || email.Length > EMAIL_AND_PASSWORD_LENGTH || !email.Contains("@"))
        {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(passwordHash) || passwordHash.Length > EMAIL_AND_PASSWORD_LENGTH)
        {
            throw new ArgumentException();
        }

        return new UserEntitie (Guid.NewGuid(), firstName, lastName, email, passwordHash);
    }
    
    
}
