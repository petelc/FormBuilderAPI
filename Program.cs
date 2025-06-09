using FormBuilderAPI.Models;
using FormBuilderAPI.Swagger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value {x} is not valid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The value for {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        (x) => $"A value for the '{x}' parameter or property was not provided.");
});


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

// builder.Services.Configure<ApiBehaviorOptions>(options =>
// {
//     options.SuppressModelStateInvalidFilter = true;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    //app.MapScalarApiReference();
    app.UseSwaggerUi();
    app.UseDeveloperExceptionPage();
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

// Minimal API
app.MapGet("/error",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] (HttpContext context) =>
    {
        var exceptionHandler =
            context.Features.Get<IExceptionHandlerPathFeature>();

        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id
              ?? context.TraceIdentifier;
        details.Type =
            "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        details.Status = StatusCodes.Status500InternalServerError;
        return Results.Problem(details);
    });

app.MapGet("/error/test",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    { throw new Exception("test"); });

app.MapGet("/cod/test",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    Results.Text("<script>" +
        "window.alert('Your client supports JavaScript!" +
        "\\r\\n\\r\\n" +
        $"Server time (UTC): {DateTime.UtcNow.ToString("o")}" +
        "\\r\\n" +
        "Client time (UTC): ' + new Date().toISOString());" +
        "</script>" +
        "<noscript>Your client does not support JavaScript</noscript>",
        "text/html"));

app.MapControllers().RequireCors("AnyOrigin");

app.Run();
