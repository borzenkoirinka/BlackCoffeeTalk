using System;
using BlackCoffeeTalk.Framework.Components;
using Microsoft.OData;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using BlackCoffeeTalk.Framework.Common.Exceptions;
using BlackCoffeeTalk.Framework.Common.Extentions;
using BlackCoffeeTalk.Shared;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    public class ExceptionHandler : ExceptionFilterAttribute
    {
        public ExceptionHandler(ApplicationEventSource eventSource)
        {
            _eventSource = eventSource;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                HttpStatusCode code;
                ODataError report = null;

                if (context.Exception is HttpResponseException)
                {
                    return;
                }
                else if (context.Exception is ServiceException)
                {
                    var e = (ServiceException)context.Exception.GetLastInnerException();

                    report = e.Error;
                    code = e.StatusCode;
                }
                else if (context.Exception is ModelStateException)
                {
                    var e = new ServiceException((ModelStateException)context.Exception);
                    report = e.Error;
                    code = e.StatusCode;
                }
                else
                {
                    report = CommunicationErrors.InternalServerError;
                    report.InnerError = new ODataInnerError(context.Exception.GetLastInnerException());

                    code = HttpStatusCode.InternalServerError;
                }

                LogException(context, code, report);

                context.Exception = new HttpResponseException(context.Request.CreateResponse(code, report));
            }
        }

        private void LogException(HttpActionExecutedContext context, HttpStatusCode code, ODataError report)
        {
            Uri uri = context.Request.RequestUri;
            String httpMethod = context.Request.Method.Method;
            String controllerName = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            String actionName = context.ActionContext.ActionDescriptor.ActionName;
            _eventSource.Error($"Exception in [{httpMethod}]{uri} ({controllerName}.{actionName})\n{code} : {report}", context.Exception);
        }

        private readonly ApplicationEventSource _eventSource;
    }

    //public class ValidateModelAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(HttpActionContext actionContext)
    //    {
    //        if (actionContext.ModelState.IsValid == false)
    //        {

    //            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, new ServiceException(actionContext.ModelState).Error);
    //        }
    //    }
    //}
}
