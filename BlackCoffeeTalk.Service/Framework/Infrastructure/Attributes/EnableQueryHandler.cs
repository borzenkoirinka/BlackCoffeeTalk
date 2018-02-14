using BlackCoffeeTalk.Framework.Common.Exceptions;
using BlackCoffeeTalk.Framework.Common.Extentions;
using BlackCoffeeTalk.Framework.Components;
using BlackCoffeeTalk.Shared;
using Microsoft.OData;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ApplicationEventSource _eventSource;
        public GlobalExceptionHandler(ApplicationEventSource eventSource)
        {
            _eventSource = eventSource;
        }

        public virtual Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context.Request.Method == HttpMethod.Get)
            {
                if (context.Exception!=null)
                {
                    HttpStatusCode code;
                    ODataError report = null;

                    //if (context.Exception is HttpResponseException)
                    //{
                    //   // return;
                    //}
                    //else 
                    if (context.Exception is ServiceException)
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
                        report.Message = context.Exception.GetLastInnerException().Message;

                        code = HttpStatusCode.InternalServerError;
                    }

                    LogException(context, code, report);

                    var response = context.Request.CreateResponse(code, report);
                    context.Result = new ResponseMessageResult(response);
                }
            }

            return Task.CompletedTask;
        }

        private void LogException(ExceptionHandlerContext context, HttpStatusCode code, ODataError report)
        {
            Uri uri = context.Request.RequestUri;
            String httpMethod = context.Request.Method.Method;
            _eventSource.Error($"Exception in [{httpMethod}]{uri}\n{code} : {report}", context.Exception);
        }
    }
}
