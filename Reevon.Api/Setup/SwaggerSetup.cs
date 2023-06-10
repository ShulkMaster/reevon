using Microsoft.OpenApi.Models;

namespace Reevon.Api.Setup;

public static class SwaggerSetup
{
    public static void SetUpSwagger(this IServiceCollection services)
    {
        
        services.AddSwaggerGen(options =>
        {
            var info = new OpenApiInfo
            {
                Title = "Reevon API",
                Version = "v1",
                Description = "Reevon API",
            };
            options.SwaggerDoc("v1", info);
        });
    }
    
    public static void EnableSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reevon API");
            options.RoutePrefix = string.Empty;
        });
    }
}