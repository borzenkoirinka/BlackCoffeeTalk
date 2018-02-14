using Swashbuckle.Swagger;
using System.Linq;
using System.Web.Http.Description;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    public class AddExtendedMetadataOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.produces = 
            new[] {
                $"application/json; odata.metadata=none",
                $"application/json; odata.metadata=minimal",
                $"application/json; odata.metadata=full"
            }.ToList();            
        }
    }
}
