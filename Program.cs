using FormBuilderAPI.Models;
using FormBuilderAPI.Swagger;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.AspNetCore;
using SwaggerThemes;
using Microsoft.AspNetCore.Builder;
//using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "FormBuilder API",
            Description = "An ASP.NET Core Web API for the form builder application",
            TermsOfService = "https://example.com/terms",
            Contact = new NSwag.OpenApiContact
            {
                Name = "Example Contact",
                Url = "https://example.com/contact"
            },
            License = new NSwag.OpenApiLicense
            {
                Name = "Example License",
                Url = "https://example.com/license"
            }
        };
    };
    options.OperationProcessors.Add(new SortOrderFilter());
    options.OperationProcessors.Add(new SortOrderColumnFilter());
});

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Define CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]!);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();

    });
    options.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    //app.MapScalarApiReference();
    app.UseSwaggerUi();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}




app.UseHttpsRedirection();

app.UseCors(); // Use the default CORS policy

app.UseAuthorization();

app.MapControllers();

app.Run();
