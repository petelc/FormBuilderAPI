using NSwag.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

using NSwag.Generation.Processors.Security;
using FormBuilderAPI.Swagger;
using FormBuilderAPI.Models;
using FormBuilderAPI.Constants;
using FormBuilderAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Kestrel configuration
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

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
    options.CacheProfiles.Add("NoCache", new CacheProfile
    {
        NoStore = true,
    });
    options.CacheProfiles.Add("Any-60", new CacheProfile
    {
        Location = ResponseCacheLocation.Any,
        Duration = 60,
    });
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new NSwag.OpenApiInfo
        {
            Version = "FormBuilder v1",
            Title = "FormBuilder API",
            Description = "An ASP.NET Core Web API for the form builder application",
            TermsOfService = "https://example.com/terms",
            Contact = new NSwag.OpenApiContact
            {
                Name = "Peter Carroll",
                Url = "https://example.com/contact"
            },
            License = new NSwag.OpenApiLicense
            {
                Name = "FormBuilder License",
                Url = "https://example.com/license"
            }
        };


    };
    options.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Description = "FormBuilder Authentication",
        Name = "FB-Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
    options.OperationProcessors.Add(new SortOrderFilter());
    options.OperationProcessors.Add(new SortOrderColumnFilter());
    options.DocumentProcessors.Add(new CustomDocumentFilter());
});

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity services
builder.Services.AddIdentity<ApiUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 12;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!))
    };
});

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

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 32 * 1024 * 1024;
    options.SizeLimit = 50 * 1024 * 1024;
});

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    //app.MapScalarApiReference();
    app.UseSwaggerUi(config =>
    {
        config.DocExpansion = "list"; // or "full" or "none"
    });
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
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.RemoveInsecureHeaders();

app.Use((context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            NoStore = true,
            NoCache = true,
        };
    return next.Invoke();
});


app.MapControllers().RequireCors("AnyOrigin");

app.Run();



