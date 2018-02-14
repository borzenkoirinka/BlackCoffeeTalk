using Microsoft.OData;

namespace BlackCoffeeTalk.Framework.Components
{
    public static class CommunicationErrors
    {
        public static ODataError InternalServerError => new ODataError() { ErrorCode = "internal_server_error", Message = "Unhandled exception is thrown." };     
        
        public static ODataError GenericError => new ODataError() { ErrorCode = "generic_error", Message = "Processing error is occured." };

        public static ODataError MethodIsNotAllowed => new ODataError() { ErrorCode = "method_is_not_allowed", Message = "Executing HTTP method is not allowed for this entity." };

        public static ODataError PutIdMismatch => new ODataError() { ErrorCode = "put_id_mismatch", Message = "Put method is required to have url id and body id match." };
        public static ODataError PatchIdProperty => new ODataError() { ErrorCode = "patch_id_mismatch", Message = "Patch method is not allowed to change entity's id." };

        public static ODataError InvalidModel => new ODataError() { ErrorCode = "invalid_model", Message = "Model has failed validation." };
        public static ODataError NullModel => new ODataError() { ErrorCode = "model_is_null", Message = "This method requires model in body." };

        public static ODataError EntityNotFound => new ODataError() { ErrorCode = "entity_not_found", Message = "Entity with specified id is not found." };


        public static ODataError CannotDetermineFeatureName => new ODataError() { ErrorCode = "unknown_feature", Message = "Cannot detemine security for requested feature." };
        public static ODataError RequestIsNotAuthenticated => new ODataError() { ErrorCode = "not_authenticated", Message = "Token was neither sent nor valid. Check your token aud, iss and exp and try again." };

        public static ODataError NotAllowed => new ODataError() { ErrorCode = "not_allowed", Message = "Neither of provided roles is allowed to use requested feature." };
    }
}
