using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using FormBuilderAPI.DTO;
using FormBuilderAPI.Models;
using FormBuilderAPI.Attributes;
using FormBuilderAPI.Constants;
using Serilog;

namespace FormBuilderAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FormController : ControllerBase
{
    static int _callCount;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FormController> _logger;


    public FormController(ILogger<FormController> logger, ApplicationDbContext context)
    {
        _context = context;
        _logger = logger;

    }


    [HttpGet(Name = "GetForm")]
    [ResponseCache(CacheProfileName = "Any-60")]
    [ManualValidationFilter]
    public async Task<ActionResult<RestDTO<Form[]>>> GetForm([FromQuery] RequestDTO<FormDTO> input)
    {
        _logger.LogInformation("Hello, world!");

        //_diagnosticContext.Set("IndexCallCount", Interlocked.Increment(ref _callCount));

        if (!ModelState.IsValid)
        {
            var details = new ValidationProblemDetails(ModelState);

            details.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            if (ModelState.Keys.Any(k => k == "PageSize"))
            {
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                details.Status = StatusCodes.Status501NotImplemented;
                return StatusCode(StatusCodes.Status501NotImplemented, details);
            }
            else
            {
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;
                return BadRequest(details);
            }
        }

        var query = _context.Forms.AsQueryable();
        if (!string.IsNullOrEmpty(input.FilterQuery))
        {
            // Assuming filterQuery is a simple string to filter FormNumber
            query = query.Where(f => f.FormNumber.Contains(input.FilterQuery));
        }
        var recordCount = await query.CountAsync();

        query = query
            .OrderBy($"{input.SortColumn} {input.SortOrder}")
            .Skip(input.PageIndex * input.PageSize)
            .Take(input.PageSize);

        return new RestDTO<Form[]>
        {
            Data = await query.ToArrayAsync(),
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            RecordCount = recordCount,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Forms", new { pageIndex = input.PageIndex, pageSize = input.PageSize }, Request.Scheme)!,
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
