namespace SlaeSolver.BLL.Security;
using BCrypt.Net;
public class PasswordHasher: IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.EnhancedHashPassword(password);
    }
    

    public bool PasswordVerification(string inputhashPassword,string hashPassword)
    {
        return BCrypt.EnhancedVerify(inputhashPassword,hashPassword);
    }
}