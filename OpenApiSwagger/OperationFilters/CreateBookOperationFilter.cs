using Microsoft.OpenApi.Models;
using OpenApiSwagger.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiSwagger.OperationFilters
{
    public class CreateBookOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if(operation.OperationId != "CreateBook")
            {
                return; 
            }

            operation.RequestBody.Content.Add("application/vnd.marvin.bookforcreationwithamountofpages+json",
                 new OpenApiMediaType()
                 {
                   // Schema = context.SchemaRegistry.GetOrRegister(typeof(BookForCreationWithAmountOfPages))
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(BookForCreationWithAmountOfPages),context.SchemaRepository)
                 });
            

        }
    }
}
