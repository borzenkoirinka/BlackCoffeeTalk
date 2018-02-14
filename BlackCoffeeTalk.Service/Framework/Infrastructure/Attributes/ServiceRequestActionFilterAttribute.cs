using System;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BlackCoffeeTalk.Framework.Common.Extentions;
using BlackCoffeeTalk.Shared;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ServiceRequestActionFilterAttribute : ActionFilterAttribute
    {
        public ServiceRequestActionFilterAttribute(ApplicationEventSource source)
        {
            _source = source;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            String method = actionContext.Request.Method.Method;
            Uri uri = actionContext.Request.RequestUri;
            String controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            String actionName = actionContext.ActionDescriptor.ActionName;

            String message = $"Request [{method}] {uri} ({controllerName}.{actionName})";
            _source.Message(message);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            String method = actionExecutedContext.Request.Method.Method;
            Uri uri = actionExecutedContext.Request.RequestUri;

            if (actionExecutedContext.Exception != null)
            {
                String controllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                String actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;

                String message = $"Exception {actionExecutedContext.Exception.GetLastInnerException().Message}: [{method}] {uri} ({controllerName}.{actionName})";
                _source.Message(message);
            }
            else
            {
                HttpStatusCode statusCode = actionExecutedContext.Response.StatusCode;

                String controllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                String actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;

                String message = $"Response {statusCode}: [{method}] {uri} ({controllerName}.{actionName})";
                _source.Message(message);
            }
        }

        private readonly ApplicationEventSource _source;
    }
}
