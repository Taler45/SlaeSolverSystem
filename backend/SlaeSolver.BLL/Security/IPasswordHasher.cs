namespace SlaeSolver.BLL.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool PasswordVerification(string hashPassword,string inputhashPassword);
}