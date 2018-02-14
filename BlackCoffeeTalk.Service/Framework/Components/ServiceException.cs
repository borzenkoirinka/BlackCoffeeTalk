using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.ModelBinding;
using BlackCoffeeTalk.Framework.Common.Exceptions;
using BlackCoffeeTalk.Framework.Common.Extentions;

namespace BlackCoffeeTalk.Framework.Components
{
    public class ServiceException : Exception
    {
        public ODataError Error { get; protected set; }
        public HttpStatusCode StatusCode { get; protected set; }

        public ServiceException(ODataError error, HttpStatusCode code = HttpStatusCode.BadRequest, Exception innerException = null)
        {
            Error = error;
            if (innerException != null)
                Error.InnerError = new ODataInnerError(innerException);

            StatusCode = code;
        }

        public ServiceException(string message, HttpStatusCode code = HttpStatusCode.BadRequest, Exception innerException = null)
        {
            Error = CommunicationErrors.GenericError;
            Error.Message = message;
            if (innerException != null)
                Error.InnerError = new ODataInnerError(innerException);

            StatusCode = code;
        }

        public ServiceException(ModelStateDictionary modelState)
        {
            StatusCode = HttpStatusCode.BadRequest;

            Error = CommunicationErrors.InvalidModel;
            Error.Details = new List<ODataErrorDetail>();

            foreach (var stateEntry in modelState)
                foreach (var error in stateEntry.Value.Errors)
                    Error.Details.Add(new ODataErrorDetail()
                    {
                        Target = stateEntry.Key.Substring(stateEntry.Key.IndexOf('.') + 1),
                        Message = string.IsNullOrEmpty(error.ErrorMessage)
                            ? error.Exception.GetLastInnerException().Message
                            : error.ErrorMessage
                    });
        }

        public ServiceException(ModelStateException modelState)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Error = CommunicationErrors.InvalidModel;
            Error.Details = modelState.Details.Select(t => new ODataErrorDetail
            {
                Message = t.Message,
                Target = t.Target

            }).ToList();
        }
    }
}
