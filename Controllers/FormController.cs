using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using FormBuilderAPI.DTO;
using FormBuilderAPI.Models;
using FormBuilderAPI.Attributes;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using FormBuilderAPI.Constants;

namespace FormBuilderAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FormController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FormController> _logger;
    private readonly IMemoryCache _cache;

    public FormController(ILogger<FormController> logger, ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }


    [HttpGet(Name = "GetForm")]
    [ResponseCache(CacheProfileName = "Any-60")]
    [ManualValidationFilter]
    public async Task<ActionResult<RestDTO<Form[]>>> GetForm([FromQuery] RequestDTO<FormDTO> input)
    {
        _logger.LogInformation("Hello, world!");

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

        Form[]? result = null;

        var cacheKey = $"forms_{input.GetType()}-{JsonSerializer.Serialize(input)}";

        if (!_cache.TryGetValue<Form[]>(cacheKey, out result))
        {
            _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

            query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            result = await query.ToArrayAsync();

            _cache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
        }
        else
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
        }


        return new RestDTO<Form[]>
        {
            Data = result!,
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
    [Authorize(Roles = RoleNames.Moderator)]
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
    [Authorize(Roles = RoleNames.Moderator)]
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

    [Authorize(Roles = RoleNames.Administrator)]
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
