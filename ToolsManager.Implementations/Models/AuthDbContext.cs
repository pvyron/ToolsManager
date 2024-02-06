using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ToolsManager.Implementations.Models;

public class AuthDbContext : IdentityDbContext<ToolsManagerUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }
}