using Microsoft.Extensions.Options;

namespace SlaeSolver.BLL.Security;

public class JWTOptions
{
    public string SecretKey { get; set; } = string.Empty;

    public int ExpireHours { get; set; }
}