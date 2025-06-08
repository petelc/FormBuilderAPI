using FormBuilderAPI.Attributes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NJsonSchema;
using System.Reflection.Metadata.Ecma335;

namespace FormBuilderAPI.Swagger;

public class SortOrderFilter : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var attributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType!.GetProperties()
                .Where(p => p.Name == context.MethodInfo.Name)
                .SelectMany(p => p.GetCustomAttributes(true))
                )
                .OfType<SortOrderValidatorAttribute>();
        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                foreach (var parameter in context.OperationDescription.Operation.Parameters)
                {
                    if (parameter.Name == "sortOrder")
                    {
                        var schema = new JsonSchema
                        {
                            Type = JsonObjectType.String,
                            Description = "Sort order, either ASC or DESC"
                        };
                        foreach (var value in attribute.AllowedValues)
                        {
                            schema.Enumeration.Add(value);
                        }

                        parameter.Schema = schema;

                    }
                }
            }
        }
        return true;
    }

}
