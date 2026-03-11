using System.Collections.Generic;
using AIStudyPlanner.Domain.Entities;

namespace AIStudyPlanner.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
