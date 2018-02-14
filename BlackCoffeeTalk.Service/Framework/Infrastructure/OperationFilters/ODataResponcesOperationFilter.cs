using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Description;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    public class ODataResponcesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var orderedEnumerable = from attr in apiDescription.GetControllerAndActionAttributes<ODataTypedSwaggerResponse>()
                                    orderby attr.StatusCode
                                    select attr;
            foreach (var current in orderedEnumerable)
            {
                Type type = null;

                var controller = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
                
                if (typeof(ControllerBase).IsAssignableFrom(controller))
                {
                    type = controller.BaseType.GetGenericArguments()[0];

                    if (current.IsCollection)
                        type = typeof(IEnumerable<>).MakeGenericType(type);
                }

                string text = ((int)current.StatusCode).ToString();
                operation.responses[text] = new Response
                {
                    description = (current.Description ?? this.InferDescriptionFrom(text)),
                    schema = ((type != null) ? schemaRegistry.GetOrRegister(type) : null)
                };

                operation.responses = operation.responses.OrderByDescending(r => r.Key).ToDictionary(p=>p.Key,p=>p.Value);
            }
        }

        private string InferDescriptionFrom(string statusCode)
        {
            HttpStatusCode httpStatusCode;
            if (Enum.TryParse<HttpStatusCode>(statusCode, true, out httpStatusCode))
            {
                return httpStatusCode.ToString();
            }
            return null;
        }
    }
}
