using BlackCoffeeTalk.Framework.Components;
using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        public static readonly string CmmFeaturesPrefix = "cmm";

        public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> RoleFeatureMap => new Dictionary<string, IReadOnlyCollection<string>>
        {
            {"Admin", new[]{ $"{CmmFeaturesPrefix}" } }
        };

        public string Feature { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!(actionContext.ControllerContext.Controller is ControllerBase))
                return true;

            var feature = Feature ?? GetFeatureName(actionContext);

            if (feature == null)
                throw new ServiceException(CommunicationErrors.CannotDetermineFeatureName, HttpStatusCode.Forbidden);

            var identity = actionContext.RequestContext.Principal?.Identity as ClaimsIdentity;

            if (identity == null)
                throw new ServiceException(CommunicationErrors.RequestIsNotAuthenticated, HttpStatusCode.Forbidden);

            var roles = identity.Claims.Where(c => c.Type == identity.RoleClaimType).Select(c => c.Value);

            var allowed = roles.SelectMany(r => RoleFeatureMap.ContainsKey(r) ? RoleFeatureMap[r] : new string[] { })
                .Any(v => feature.StartsWith(v));

            if (!allowed)
            {
                var error = CommunicationErrors.NotAllowed;
                error.Details = new List<ODataErrorDetail> {
                    new ODataErrorDetail{Target = feature, Message = $"Provided roles: {string.Join(",",roles)}" }
                };

                throw new ServiceException(error, HttpStatusCode.Forbidden);
            }

            return true;
        }

        protected string GetFeatureName(HttpActionContext actionContext)
        {
            var method = actionContext.Request.Method;
            var controller = (ControllerBase)actionContext.ControllerContext.Controller;

            var modelName = controller.ModelType.Name.ToLowerInvariant();
            var primaryAction = method == HttpMethod.Get ? "read" : "write";

            string secondaryAction = null;

            if (method == HttpMethod.Put || method.Method == "PATCH")
                secondaryAction = actionContext.ActionDescriptor.ActionName == "PutSingle" ? "single" : "all";
            else if (method == HttpMethod.Delete)
                secondaryAction = actionContext.ActionDescriptor.ActionName == "DeleteSingle" ? "single" : "all";
            else if (method == HttpMethod.Post)
                secondaryAction = "create";
            else if (method == HttpMethod.Get)
                secondaryAction = actionContext.ActionDescriptor.ActionName == "GetSingle" ? "single" : "all";
            else
                return null;

            return $"{CmmFeaturesPrefix}.{modelName}.{primaryAction}.{secondaryAction}";
        }
    }
}
