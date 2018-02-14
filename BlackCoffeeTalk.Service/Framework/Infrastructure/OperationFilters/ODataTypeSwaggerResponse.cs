using System;
using System.Net;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ODataTypedSwaggerResponse : Attribute
    {
        public bool IsCollection { get; set; }
        public string Description { get; set; }
        public HttpStatusCode StatusCode { get; private set; }

        public ODataTypedSwaggerResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
