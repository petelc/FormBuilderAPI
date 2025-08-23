using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace FormBuilderAPI.Swagger;

internal class CustomDocumentFilter : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        // Custom logic to modify the OpenAPI document can be added here.
        // For example, adding custom security schemes or modifying existing ones.
        context.Document.Info.Title = "FormBuilder Web API";
        context.Document.Info.Description = "The FormBuilder API provides a set of endpoints for managing form json schemes.";
    }
}
