using Microsoft.EntityFrameworkCore;
using FormBuilderAPI.Constants;
using FormBuilderAPI.DTO;
using FormBuilderAPI.Models;
using HotChocolate.Authorization;

namespace FormBuilderAPI.GraphQL;

public class Mutation
{
    [Serial]
    [Authorize(Roles = new[] { RoleNames.Moderator })]
    public async Task<Form> CreateFormAsync([Service] ApplicationDbContext context, FormDTO model)
    {
        var form = new Form
        {
            FormNumber = model.FormNumber,
            FormTitle = model.FormTitle,
            FormOwnerDivision = model.FormOwnerDivision!,
            FormOwner = model.FormOwner!,
            Version = model.Version!,
            CreatedDate = model.CreatedDate ?? DateTime.UtcNow,
            RevisedDate = model.RevisedDate ?? DateTime.UtcNow,
            ConfigurationPath = model.ConfigurationPath
        };

        context.Forms.Update(form);
        await context.SaveChangesAsync();

        return form;
    }

    [Serial]
    [Authorize(Roles = new[] { RoleNames.Moderator })]
    public async Task<Form> UpdateFormAsync([Service] ApplicationDbContext context, FormDTO model)
    {
        var form = await context.Forms.Where(b => b.Id == model.Id).FirstOrDefaultAsync();

        if (form == null)
        {
            throw new Exception($"Form with ID {model.Id} not found.");
        }

        form.FormNumber = model.FormNumber;
        form.FormTitle = model.FormTitle;
        form.FormOwnerDivision = model.FormOwnerDivision!;
        form.FormOwner = model.FormOwner!;
        form.Version = model.Version!;
        form.CreatedDate = model.CreatedDate ?? DateTime.UtcNow;
        form.RevisedDate = model.RevisedDate ?? DateTime.UtcNow;
        form.ConfigurationPath = model.ConfigurationPath;

        context.Forms.Update(form);
        await context.SaveChangesAsync();

        return form;
    }

    [Serial]
    [Authorize(Roles = new[] { RoleNames.Administrator })]
    public async Task DeleteFormAsync([Service] ApplicationDbContext context, int id)
    {
        var form = await context.Forms.Where(b => b.Id == id).FirstOrDefaultAsync();

        if (form != null)
        {
            context.Forms.Remove(form);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception($"Form with ID {id} not found.");
        }

    }
}
