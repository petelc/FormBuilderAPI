using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FormBuilderAPI.Constants;
using FormBuilderAPI.Models;
using FormBuilderAPI.Models.CSV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormBuilderAPI.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
[Route("api/[controller]/[action]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SeedController> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApiUser> _userManager;

    public SeedController(
        ILogger<SeedController> logger,
        ApplicationDbContext context,
        IWebHostEnvironment env,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApiUser> userManager)
    {
        _context = context;
        _logger = logger;
        _roleManager = roleManager;
        _userManager = userManager;
        _env = env;
    }

    [HttpPut]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<IActionResult> FormData()
    {
        // SETUP
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("en-US"))
        {
            HasHeaderRecord = true,
            Delimiter = ",",
        };

        using var reader = new StreamReader(
                System.IO.Path.Combine(_env.ContentRootPath, "Models/CSV/fbData.csv"));
        using var csv = new CsvReader(reader, config);
        var existingForms = await _context.Forms
            .ToDictionaryAsync(bg => bg.Id);
        var existingDomains = await _context.Domains
            .ToDictionaryAsync(d => d.Type);
        var now = DateTime.Now;

        // EXECUTE
        var records = csv.GetRecords<FrmRecord>();
        var skippedRows = 0;
        foreach (var record in records)
        {
            if (!record.ID.HasValue
                || string.IsNullOrEmpty(record.FormTitle)
                || existingForms.ContainsKey(record.ID.Value))
            {
                skippedRows++;
                continue;
            }

            var form = new Form()
            {
                Id = record.ID.Value,
                FormNumber = record.FormNumber ?? string.Empty,
                FormTitle = record.FormTitle ?? string.Empty,
                FormOwnerDivision = record.FormOwnerDivision ?? string.Empty,
                FormOwner = record.FormOwner ?? string.Empty,
                Version = record.Version ?? string.Empty,
                CreatedDate = now,
                RevisedDate = now,
                ConfigurationPath = record.ConfigurationPath ?? string.Empty
            };
            form.RevisedDate = now;
            _context.Forms.Add(form);

            if (!string.IsNullOrEmpty(record.Domains))
                foreach (var type in record.Domains
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var domain = existingDomains.GetValueOrDefault(type);
                    if (domain == null)
                    {
                        domain = new Domain()
                        {
                            Type = type,
                            CreatedDate = now,
                            LastModifiedDate = now
                        };
                        _context.Domains.Add(domain);
                        existingDomains.Add(type, domain);
                    }
                    _context.Forms_Domains.Add(new Forms_Domains()
                    {
                        Form = form,
                        Domain = domain,
                        CreatedDate = now
                    });
                }
        }

        // SAVE
        //using var transaction = _context.Database.BeginTransaction();
        //_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Forms ON");
        await _context.SaveChangesAsync();
        //_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Forms OFF");
        //transaction.Commit();

        // RECAP
        return new JsonResult(new
        {
            Forms = _context.Forms.Count(),
            Domains = _context.Domains.Count(),
            SkippedRows = skippedRows
        });
    }

    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<IActionResult> AuthData()
    {
        int rolesCreated = 0;
        int usersAddedToRoles = 0;

        if (!await _roleManager.RoleExistsAsync(RoleNames.Moderator))
        {

            await _roleManager.CreateAsync(new IdentityRole(RoleNames.Moderator));
            rolesCreated++;
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.Administrator))
        {
            await _roleManager.CreateAsync(new IdentityRole(RoleNames.Administrator));
            rolesCreated++;
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.SuperAdmin))
        {
            await _roleManager.CreateAsync(new IdentityRole(RoleNames.SuperAdmin));
            rolesCreated++;
        }

        var testModerator = await _userManager.FindByNameAsync("TestModerator");
        if (testModerator != null && !await _userManager.IsInRoleAsync(testModerator, RoleNames.Moderator))
        {
            await _userManager.AddToRoleAsync(testModerator, RoleNames.Moderator);
            usersAddedToRoles++;
        }

        var testAdmin = await _userManager.FindByNameAsync("TestAdministrator");
        if (testAdmin != null && !await _userManager.IsInRoleAsync(testAdmin, RoleNames.Administrator))
        {
            await _userManager.AddToRoleAsync(testAdmin, RoleNames.Moderator);
            await _userManager.AddToRoleAsync(testAdmin, RoleNames.Administrator);
            usersAddedToRoles++;
        }

        var testSuperAdmin = await _userManager.FindByNameAsync("TestSuperAdmin");
        if (testSuperAdmin != null && !await _userManager.IsInRoleAsync(testSuperAdmin, RoleNames.SuperAdmin))
        {
            await _userManager.AddToRoleAsync(testSuperAdmin, RoleNames.Moderator);
            await _userManager.AddToRoleAsync(testSuperAdmin, RoleNames.Administrator);
            await _userManager.AddToRoleAsync(testSuperAdmin, RoleNames.SuperAdmin);
            usersAddedToRoles++;
        }


        return new JsonResult(new
        {
            RolesCreated = rolesCreated,
            UsersAddedToRoles = usersAddedToRoles
        });
    }

}
