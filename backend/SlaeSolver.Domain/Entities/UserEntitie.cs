using SlaeSolver.Domain.Exceptions;

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

    public static string NormalizeEmail(string email)
    {
        return email.ToLowerInvariant().Trim();
    }
    
    public static UserEntitie Create(string firstName, string lastName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > USER_NAME_LENGTH)
        {
            throw new DomainException("Ім'я порожнє або довше 100 символів");
        }

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > USER_NAME_LENGTH)
        {
            throw new DomainException("Прізвище порожнє або довше 100;");
        }

        if (string.IsNullOrWhiteSpace(email) || email.Length > EMAIL_AND_PASSWORD_LENGTH || !email.Contains("@"))
        {
            throw new DomainException("email порожній, довший 255 або без @");
        }

        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > EMAIL_AND_PASSWORD_LENGTH)
        {
            throw new DomainException("хеш пароля порожній");
        }

        return new UserEntitie (Guid.NewGuid(), firstName, lastName, NormalizeEmail(email), passwordHash);
    }
    
    
}
