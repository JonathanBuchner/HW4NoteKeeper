using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HW4NoteKeeperEx2.Infrastructure.Services
{
    /// <summary>
    /// Register Swagger for the application. Make sure both methods are included on program.cs file for intended swagger use.
    /// </summary>
    public class RegistererSwagger
    {
        /// <summary>
        /// Add Swagger to the application.  This method is used before the application is built.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Buchner Note Keeper App", Version = "v1" });

                // Add documentation via C# XML Comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Use Swagger for the application.  This method is used after the application is built.
        /// </summary>
        /// <param name="app"></param>
        public static void UseSwagger(WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Buchner Note Keeper v1");
                options.RoutePrefix = string.Empty;
            });
        }
    }
}
