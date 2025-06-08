using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using FormBuilderAPI.Attributes;
using NJsonSchema;

namespace FormBuilderAPI.Swagger;

public class SortOrderColumnFilter : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var attributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType!.GetProperties()
                .Where(p => p.Name == context.MethodInfo.Name)
                .SelectMany(p => p.GetCustomAttributes(true))
                )
                .OfType<SortColumnValidatorAttribute>();
        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                var pattern = string.Join("|", attribute.EntityType.GetProperties().Select(p => p.Name));
                foreach (var parameter in context.OperationDescription.Operation.Parameters)
                {
                    if (parameter.Name == "sortColumn")
                    {
                        var schema = new JsonSchema
                        {
                            Type = JsonObjectType.String,
                            Description = $"Sort column, one of: {pattern}"
                        };
                        schema.Pattern = $"^({pattern})$";
                        parameter.Schema = schema;

                    }
                }

            }
        }
        return true;
    }

}
