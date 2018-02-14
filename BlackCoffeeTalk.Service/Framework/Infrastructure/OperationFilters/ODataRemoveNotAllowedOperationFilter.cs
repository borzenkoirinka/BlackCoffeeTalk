using Swashbuckle.Swagger;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Description;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    public class ODataRemoveNotAllowedOperationFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var apiDescription in apiExplorer.ApiDescriptions)
            {
                if (apiDescription.Documentation.Contains(" DefaultSettings"))
                {
                }
                var action = apiDescription.ActionDescriptor;
                var controller = action.ControllerDescriptor.ControllerType;
                if (controller.BaseType.IsGenericType && controller.BaseType.GetGenericTypeDefinition() == typeof(ControllerBase<>))
                {
                    var entityType = controller.BaseType.GetGenericArguments()[0];

                    var path = "/" + apiDescription.RelativePath;

                    MethodInfo method = null;

                    if (apiDescription.HttpMethod == HttpMethod.Get)
                    {
                        if (!swaggerDoc.paths[path].get.operationId.Contains("Single"))
                        {
                            method = controller.GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance,
                                Type.DefaultBinder, new[] { typeof(IQueryable<>).MakeGenericType(entityType) },
                                null);
                            if (IsOverritten(method))
                            {
                                swaggerDoc.paths[path].delete = MapOperation(swaggerDoc.paths[path].get);
                                swaggerDoc.paths[path].delete.operationId = swaggerDoc.paths[path].delete.operationId.Replace("Get", "Delete");
                            }
                            method = controller.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance,
                                Type.DefaultBinder, new[] { typeof(IQueryable<>).MakeGenericType(entityType), entityType },
                                null);
                            if (IsOverritten(method))
                            {
                                swaggerDoc.paths[path].put = MapOperation(swaggerDoc.paths[path].get);
                                swaggerDoc.paths[path].put.operationId = swaggerDoc.paths[path].get.operationId.Replace("Get", "Put");
                                var ss1 = swaggerDoc.paths.Where(t => t.Key.StartsWith(path) && t.Value.put.parameters.Any(z => z.@in == "body")).Select(t => t.Value).FirstOrDefault();
                                var ss2 = ss1.put.parameters.FirstOrDefault(z => z.@in == "body");

                                swaggerDoc.paths[path].put.parameters.Add(ss2);
                            }
                        }
                        method = controller.GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new Type[] { }, null);
                        if (!IsOverritten(method))
                            swaggerDoc.paths[path].get = null;



                    }
                    else if (apiDescription.HttpMethod == HttpMethod.Post)
                    {
                        method = controller.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] { entityType }, null);
                        if (!IsOverritten(method))
                            swaggerDoc.paths[path].post = null;
                    }
                    else if (apiDescription.HttpMethod == HttpMethod.Delete)
                    {
                        method = controller.GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance,
                            Type.DefaultBinder, new[] { typeof(EntityKey<>).MakeGenericType(entityType) }, null);

                        if (!IsOverritten(method))
                            swaggerDoc.paths[path].delete = null;
                    }
                    else if (apiDescription.HttpMethod == HttpMethod.Put || apiDescription.HttpMethod.Method == "PATCH")
                    {
                        method = controller.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] { entityType }, null);
                        if (!IsOverritten(method))
                        {
                            swaggerDoc.paths[path].put = null;
                            swaggerDoc.paths[path].patch = null;
                        }
                    }
                }
            }
        }

        public static bool IsOverritten(MethodInfo m)
        {
            return m != null && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        private Operation MapOperation(Operation op)
        {
            var newOperation = new Operation();
            newOperation.parameters = op.parameters?.Select(t => t).ToList();
            newOperation.operationId = op.operationId;
            newOperation.produces = op.produces?.Select(t => t).ToList();
            newOperation.summary = op.summary;
            newOperation.tags = op.tags?.Select(t => t).ToList();
            newOperation.deprecated = op.deprecated;
            newOperation.responses = op.responses; //todo 
            return newOperation;
        }
    }
}
