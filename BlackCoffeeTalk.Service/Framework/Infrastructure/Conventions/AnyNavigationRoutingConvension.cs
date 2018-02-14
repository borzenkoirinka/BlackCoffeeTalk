using BlackCoffeeTalk.Framework.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing.Conventions;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    class AnyNavigationRoutingConvension : NavigationRoutingConvention
    {
        public override string SelectAction(System.Web.OData.Routing.ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var method = controllerContext.Request.Method;

            if (odataPath.PathTemplate == "~/entityset/key/navigation" || odataPath.PathTemplate == "~/entityset/key/navigation/$count" || odataPath.PathTemplate == "~/entityset/key/cast/navigation" || odataPath.PathTemplate == "~/entityset/key/cast/navigation/$count" || odataPath.PathTemplate == "~/singleton/navigation" || odataPath.PathTemplate == "~/singleton/navigation/$count" || odataPath.PathTemplate == "~/singleton/cast/navigation" || odataPath.PathTemplate == "~/singleton/cast/navigation/$count")
            {
                NavigationPropertySegment navigationPropertySegment = (odataPath.Segments.Last<ODataPathSegment>() as NavigationPropertySegment) ?? (odataPath.Segments[odataPath.Segments.Count - 2] as NavigationPropertySegment);
                IEdmNavigationProperty navigationProperty = navigationPropertySegment.NavigationProperty;
                IEdmEntityType edmEntityType = navigationProperty.DeclaringType as IEdmEntityType;
                if (navigationProperty.TargetMultiplicity() != EdmMultiplicity.Many && method == HttpMethod.Post)
                {
                    return null;
                }
                if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many && (method == HttpMethod.Put || "PATCH" == method.Method.ToUpperInvariant()))
                {
                    return null;
                }
                if (odataPath.Segments.Last<ODataPathSegment>() is CountSegment && method != HttpMethod.Get)
                {
                    return null;
                }
                if (edmEntityType != null)
                {
                    controllerContext.RouteData.Values["navigation"] = navigationProperty.Name;

                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        var key = ((KeySegment)odataPath.Segments[1]).ToEntityKey();
                        controllerContext.RouteData.Values["selector"] = key;

                        controllerContext.RouteData.Values["key"] = odataPath.Segments[1] as KeySegment;

                        
                    }

                    return "GetNavigation";
                }
            }
            return null;
        }
    }
}
