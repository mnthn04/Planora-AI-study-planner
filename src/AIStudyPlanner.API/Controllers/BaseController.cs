using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AIStudyPlanner.API.Controllers
{
    public class BaseController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
