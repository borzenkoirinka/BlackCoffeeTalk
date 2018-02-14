using BlackCoffeeTalk.Framework.Extensions;
using Microsoft.OData.UriParser;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing.Conventions;

namespace BlackCoffeeTalk.Framework
{
    public class SingleEntityRoutingConvention : EntityRoutingConvention
    {
        public override string SelectController(System.Web.OData.Routing.ODataPath odataPath, HttpRequestMessage request)
        {
            return base.SelectController(odataPath, request);
        }

        public override string SelectAction(System.Web.OData.Routing.ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var httpMethod = controllerContext.Request.Method.ToString().ToUpperInvariant();

            string httpMethodName;
            if (httpMethod == "DELETE" || httpMethod == "PUT")
            {
                if (odataPath.PathTemplate == "~/entityset")
                {
                    httpMethodName = httpMethod.ToLower();

                    var action = actionMap.FirstOrDefault(a => a.Key.ToLower() == httpMethodName)?.FirstOrDefault();
                    if (action != null)
                    {
                        return action.ActionName;
                    }
                }
            }

            if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast")
            {

                switch (httpMethod)
                {
                    case "GET":
                        httpMethodName = "GetSingle";
                        break;
                    case "PUT":
                        httpMethodName = "PutSingle";
                        break;
                    case "PATCH":
                    case "MERGE":
                        httpMethodName = "Patch";
                        break;
                    case "DELETE":
                        httpMethodName = "DeleteSingle";
                        break;
                    default:
                        return null;
                }

                var action = actionMap.FirstOrDefault(a => a.Key == httpMethodName)?.FirstOrDefault();

                if (action != null)
                {
                    var keySegment = ((KeySegment)odataPath.Segments[1]);
                    var key = keySegment.ToEntityKey();
                    controllerContext.RouteData.Values["selector"] = key;

                    foreach (var kv in keySegment.Keys)
                        controllerContext.RouteData.Values[kv.Key] = kv.Value;

                    return action.ActionName;
                }
            }

            return null;
        }
    }
}
