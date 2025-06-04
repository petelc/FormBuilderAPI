using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using FormBuilderAPI.DTO;
using FormBuilderAPI.Models;

namespace FormBuilderAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FormController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FormController> _logger;
    public FormController(ILogger<FormController> logger, ApplicationDbContext context)
    {
        _context = context;
        _logger = logger;
    }


    [HttpGet(Name = "GetForm")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<RestDTO<Form[]>> GetForm(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = "FormNumber",
        string? sortOrder = "ASC",
        string? filterQuery = null
        )
    {
        var query = _context.Forms.AsQueryable();
        if (!string.IsNullOrEmpty(filterQuery))
        {
            // Assuming filterQuery is a simple string to filter FormNumber
            query = query.Where(f => f.FormNumber.Contains(filterQuery));
        }
        var recordCount = await query.CountAsync();

        query = query
            .OrderBy($"{sortColumn} {sortOrder}")
            .Skip(pageIndex * pageSize)
            .Take(pageSize);

        return new RestDTO<Form[]>
        {
            Data = await query.ToArrayAsync(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = recordCount,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Forms", new { pageIndex, pageSize }, Request.Scheme)!,
                    "self",
                    "GET"
                )
            },
        };
    }

    /// <summary>
    /// Creates a new form with the provided FormDTO.
    /// </summary>
    /// <param name="formDto"></param>
    /// <returns>A new RestDTO containing form data</returns>
    [HttpPost(Name = "CreateForm")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Form?>> CreateForm(FormDTO formDto)
    {
        var form = new Form
        {
            FormNumber = formDto.FormNumber,
            FormTitle = formDto.FormTitle,
            FormOwnerDivision = formDto.FormOwnerDivision!,
            FormOwner = formDto.FormOwner!,
            Version = formDto.Version!,
            CreatedDate = DateTime.UtcNow,
            RevisedDate = DateTime.UtcNow,
            ConfigurationPath = formDto.ConfigurationPath!
        };

        _context.Forms.Add(form);
        await _context.SaveChangesAsync();

        return new RestDTO<Form?>
        {
            Data = form,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        "Forms",
                        new { id = form.Id },
                        Request.Scheme)!,
                    "self",
                    "GET"
                )
            },
        };
    }


    /// <summary>
    /// Updates an existing form with the provided FormDTO.
    /// </summary>
    /// <param name="formDto">Form DTO object</param>
    /// <returns>Status Code 200 if successful, 404 if not found</returns>  
    [HttpPut(Name = "UpdateForm")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Form?>> UpdateForm(FormDTO formDto)
    {
        var form = await _context.Forms
            .FirstOrDefaultAsync(f => f.Id == formDto.Id);

        if (form != null)
        {
            if (!string.IsNullOrEmpty(formDto.FormNumber))
            {
                form.FormNumber = formDto.FormNumber;
            }
            if (!string.IsNullOrEmpty(formDto.FormOwnerDivision))
            {
                form.FormOwnerDivision = formDto.FormOwnerDivision;
            }
            if (!string.IsNullOrEmpty(formDto.FormOwner))
            {
                form.FormOwner = formDto.FormOwner;
            }
            if (!string.IsNullOrEmpty(formDto.Version))
            {
                form.Version = formDto.Version;
            }
            if (formDto.RevisedDate.HasValue)
            {
                form.RevisedDate = formDto.RevisedDate.Value;
            }
            if (!string.IsNullOrEmpty(formDto.ConfigurationPath))
            {
                form.ConfigurationPath = formDto.ConfigurationPath;
            }
            await _context.SaveChangesAsync();
        }

        return new RestDTO<Form?>
        {
            Data = form,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        "Forms",
                        formDto,
                        Request.Scheme)!,
                    "self",
                    "GET"
                )
            },

        };
    }

    [HttpDelete(Name = "DeleteForm")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Form?>> DeleteForm(int id)
    {
        var form = await _context.Forms.FindAsync(id);
        if (form != null)
        {
            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();

        }

        return new RestDTO<Form?>
        {
            Data = form,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        "Forms",
                        id,
                        Request.Scheme)!,
                    "self",
                    "DELETE"
                )
            },

        };
    }
}
