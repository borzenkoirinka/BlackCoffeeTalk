using System;
using System.Collections.Generic;

namespace BlackCoffeeTalk.Framework.Common.Exceptions
{
    public class ModelStateException:Exception
    {
        public ModelStateException(IEnumerable<ModelState> errorMessages)
        {
            Details = errorMessages;
            Message = "Model has failed validation.";
        }

        public IEnumerable<ModelState> Details { get; set; }

        public string Message { get; set; }

    }

    public class ModelState
    {
        public string Target { get; set; }
        public string Message { get; set; }
    }
}
