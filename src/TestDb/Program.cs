using System;
using System.Linq;
using AIStudyPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Newtonsoft.Json;

namespace TestDb
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AIStudyPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;");

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                var user = context.Users.FirstOrDefault();
                if (user != null)
                {
                    Console.WriteLine($"User: {user.Email} (Id: {user.Id})");

                    var subjects = context.Subjects
                        .Where(s => s.UserId == user.Id)
                        .Include(s => s.Topics)
                        .Include(s => s.StudyPlan)
                        .ThenInclude(p => p.StudyTasks)
                        .ToList();

                    Console.WriteLine($"Found {subjects.Count} subjects.");

                    try
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        };
                        var json = JsonConvert.SerializeObject(subjects, settings);
                        Console.WriteLine($"JSON length: {json.Length}");
                        Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Serialization Error: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("No users found.");
                }
            }
        }
    }
}
