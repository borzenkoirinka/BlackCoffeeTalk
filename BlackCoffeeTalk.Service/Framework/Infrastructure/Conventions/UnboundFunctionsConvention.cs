using Microsoft.OData.UriParser;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing.Conventions;
using BlackCoffeeTalk.Framework.Common;

namespace BlackCoffeeTalk.Framework.Infrastructure.Conventions
{
   public class UnboundFunctionsConvention : NavigationRoutingConvention
    {
        public override string SelectController(System.Web.OData.Routing.ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath.PathTemplate.StartsWith("~/unboundfunction"))
            {
                var item =
                    odataPath.Segments.FirstOrDefault();
                if (item.Identifier.StartsWith(Constants.ValidationFunctionName))
                {
                    return Constants.ExtendedMetadataControllerName;
                }
            }
            return null;
        }

        public override string SelectAction(System.Web.OData.Routing.ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath.PathTemplate.StartsWith("~/unboundfunction"))
            {
                var funcId = ((OperationImportSegment)odataPath.Segments[0]).Identifier;
                var action = actionMap.FirstOrDefault(a => a.Key == funcId)?.FirstOrDefault();

                if (action == null)
                    return null;

                return action.ActionName;
            }

            return null;
        }
    }
}
