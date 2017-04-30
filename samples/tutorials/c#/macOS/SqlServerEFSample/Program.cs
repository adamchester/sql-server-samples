using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Extensions;
using SerilogTimings.Extensions;

namespace SqlServerEFSample
{
    class Program
    {
        static readonly string ConnectionString = new SqlConnectionStringBuilder {
            DataSource = "192.168.99.100", UserID = "sa",
            Password = "yourStrong(!)Password",
            InitialCatalog = "EFSampleDB",
        }.ConnectionString;

        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .Destructure.ByTransforming<User>(u => u.AsLogEntry())
                .Destructure.ByTransforming<Task>(t => t.AsLogEntry())
                .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(log, dispose: false);

            log.Information("** C# CRUD sample with Entity Framework Core and SQL Server **");

            try
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(ConnectionString)
                    .EnableSensitiveDataLogging()
                    //.UseLoggerFactory(loggerFactory)
                    .Options;

                using (EFSampleContext context = new EFSampleContext(options))
                {
                    using (log.TimeOperation("Deleting database"))
                        context.Database.EnsureDeleted();
                    
                    using (log.TimeOperation("Creating database"))
                        context.Database.EnsureCreated();

                    // Create demo: Create a User instance and save it to the database
                    User newUser = new User { FirstName = "Anna", LastName = "Shrestinian" };
                    context.Users.Add(newUser);
                    context.SaveChanges();
                    log.Information("Created {@User}", newUser);

                    // Create demo: Create a Task instance and save it to the database
                    Task newTask = new Task() { Title = "Ship Helsinki", IsComplete = false, DueDate = DateTime.Parse("01-APR-2017") };
                    context.Tasks.Add(newTask);
                    context.SaveChanges();
                    log.Information("Created {@Task}", newTask);

                    // Association demo: Assign task to user
                    newTask.AssignedTo = newUser;
                    context.SaveChanges();
                    log.Information("Assigned Task: {TaskTitle} to user {UserFullName}",
                        newTask.Title, newUser.GetFullName());

                    // Read demo: find incomplete tasks assigned to user 'Anna'
                    const string firstNameEqualsCriteria = "Anna";
                    var query = from t in context.Tasks
                                where t.IsComplete == false &&
                                    t.AssignedTo.FirstName == firstNameEqualsCriteria
                                select t;
                    log.Information(
                        "Incomplete tasks assigned to {FirstNameEquals} are {@Tasks}",
                        firstNameEqualsCriteria, query);

                    // Update demo: change the 'dueDate' of a task
                    Task taskToUpdate = context.Tasks.First(); // get the first task
                    log.Information("Updating {@TaskToUpdate}", taskToUpdate);
                    taskToUpdate.DueDate = DateTime.Parse("30-JUN-2016");
                    context.SaveChanges();
                    log.Information("{FieldName} changed {@TaskToUpdate}",
                        "dueDate", taskToUpdate);

                    // Delete demo: delete all tasks with a dueDate in 2016
                    log.Information("Deleting all tasks with a dueDate in 2016");
                    DateTime dueDate2016 = DateTime.Parse("01-JAN-2017");
                    query = from t in context.Tasks
                            where t.DueDate < dueDate2016
                            select t;
                    foreach(Task t in query)
                    {
                        log.Information("Deleting {@Task}", t);
                        context.Tasks.Remove(t);
                    }
                    context.SaveChanges();

                    // Show tasks after the 'Delete' operation - there should be 0 tasks
                    List<Task> tasksAfterDelete = (from t in context.Tasks select t).ToList<Task>();
                    log.Information("Tasks after delete are {@Tasks}", tasksAfterDelete);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Failed");
            }

            log.Information("All done");
        }
    }
}