using BlackCoffeeTalk.Framework.Components;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Extensions;
using BlackCoffeeTalk.Framework.Extensions;
using Microsoft.OData;
using Swashbuckle.Swagger.Annotations;
using BlackCoffeeTalk.Framework.Infrastructure;
using BlackCoffeeTalk.Shared;
using System.Web.OData.Results;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Web.OData.Query;
using System.Linq.Expressions;

namespace BlackCoffeeTalk.Framework
{
    public abstract class ControllerBase : ODataController
    {
        public abstract Type ModelType { get; }
    }

    public abstract class ControllerBase<TEntity> : ControllerBase
        where TEntity : class
    {
        public override Type ModelType => typeof(TEntity);

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            IServiceProvider container = controllerContext.Request.GetRequestContainer();
            Logger = (ApplicationEventSource)container.GetService(typeof(ApplicationEventSource)); ;
            base.Initialize(controllerContext);
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 1000, MaxOrderByNodeCount = 10, MaxAnyAllExpressionDepth=5)]
        [ActionName("Get")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK, IsCollection = true)]
        public IHttpActionResult GetHandler()
        {
            return Ok(Read());
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 1000)]
        [ActionName("GetSingle")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ODataError), Description = "Entity with specified Id was not found.")]
        public IHttpActionResult GetSingleHandler(EntityKey<TEntity> entityKey)
        {
            var entity = Read(entityKey);

            if (entity.Any())
                return Ok(SingleResult.Create(entity));
            else
                throw new ServiceException(CommunicationErrors.EntityNotFound, HttpStatusCode.NotFound);

        }

        [HttpPost]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 1000, MaxOrderByNodeCount = 10, MaxAnyAllExpressionDepth = 5)]
        [ActionName("Post")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ODataError), Description = "Submitted entity is invalid.")]
        public IHttpActionResult PostHandler(TEntity model)
        {
            BeforeValidationCheck();

            if (!ModelState.IsValid)
                throw new ServiceException(ModelState);

            if (model == null)
                throw new ServiceException(CommunicationErrors.NullModel, HttpStatusCode.BadRequest);
            
            return Created(Create(model));
        }

        protected virtual void BeforeValidationCheck()
        {
        }

        protected void SkipValidationForNestedProperties<TReturn>(Expression<Func<TEntity, TReturn>> expression)
        {
            ModelState.SkipIfContainsNestedProperties(expression);
        }

        [HttpPut]
        [EnableQuery]
        [ActionName("PutSingle")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ODataError), Description = "Submitted entity is invalid.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ODataError), Description = "Entity with specified Id was not found.")]
        public IHttpActionResult PutSingleHandler(EntityKey<TEntity> entityKey, TEntity model)
        {
            CanUpdate(entityKey, model);

            BeforeValidationCheck();

            if (!ModelState.IsValid)
                throw new ServiceException(ModelState);

            if (model == null)
                throw new ServiceException(CommunicationErrors.NullModel, HttpStatusCode.BadRequest);

            if (!entityKey.IsMatch(model))
                throw new ServiceException(CommunicationErrors.PutIdMismatch, HttpStatusCode.BadRequest);

            if (ModelState.Keys.Any() && ModelState.Keys.All(k => !k.Contains("[")))
                throw new ServiceException(ModelState);

            return Updated(Update(model));
        }

        /// <summary>
        /// Each controller can decide which errors he doesn't want to check
        /// </summary>
        /// <param name="entityKey"></param>
        /// <param name="model"></param>
        protected virtual void CanUpdate(EntityKey<TEntity> entityKey, TEntity model)
        {
        }

        [HttpPut]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 1000)]
        [ActionName("Put")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK, IsCollection = true)]
        public IHttpActionResult PutHandler(ODataQueryOptions<TEntity> query, TEntity model)
        {
            var result = (IQueryable<TEntity>)query.ApplyTo(from item in Read() select item);
            Update(result, model);
            return StatusCode(HttpStatusCode.NoContent);
        }


        [HttpPatch]
        [EnableQuery]
        [ActionName("Patch")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ODataError), Description = "Submitted patch is invalid.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ODataError), Description = "Entity with specified Id was not found.")]
        public IHttpActionResult PatchHandler(EntityKey<TEntity> entityKey, Delta<TEntity> patch)
        {
            if (patch == null)
                throw new ServiceException(CommunicationErrors.NullModel, HttpStatusCode.BadRequest);

            return Updated(Patch(entityKey, patch));
        }

        [HttpDelete]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 1000)]
        [ActionName("Delete")]
        [SwaggerResponseRemoveDefaults]
        [ODataTypedSwaggerResponse(HttpStatusCode.OK, IsCollection = true)]
        public IHttpActionResult DeleteHandler(ODataQueryOptions<TEntity> query)
        {
            var items = query.ApplyTo(from item in Read() select item);
            var itemsToDelete = (IQueryable<TEntity>)items;
            Delete(itemsToDelete);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ActionName("DeleteSingle")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ODataError), Description = "Entity with specified Id was not found.")]
        public IHttpActionResult DeleteSingleHandler(EntityKey<TEntity> entityKey)
        {
            Delete(entityKey);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        public IHttpActionResult GetNavigation(EntityKey<TEntity> entityKey, string navigation)
        {
            var prop = typeof(TEntity).GetProperty(navigation);

            var data = Read(entityKey).FirstOrDefault();
            if (data == null)
                throw new ServiceException(CommunicationErrors.EntityNotFound, HttpStatusCode.NotFound);

            var propValue = prop.GetValue(data);

            if (propValue == null)
                return StatusCode(HttpStatusCode.NoContent);

            var resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(prop.PropertyType);

            return (IHttpActionResult)Activator.CreateInstance(resultType, propValue, this);
        }

        protected virtual TEntity Patch(EntityKey<TEntity> key, Delta<TEntity> patch)
        {
            if (patch.GetChangedPropertyNames().Any(cpn => key.Keys.Any(t=>t.Key==cpn)))
                throw new ServiceException(CommunicationErrors.PatchIdProperty, HttpStatusCode.BadRequest);

            var model = Read(key).FirstOrDefault();
            if (model == null)
                throw new ServiceException(CommunicationErrors.EntityNotFound, HttpStatusCode.NotFound);

            patch.Patch(model);
            return Update(model);
        }

        protected virtual TEntity Update(TEntity model)
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }
        
        protected virtual TEntity Update(IQueryable<TEntity> query, TEntity model)
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }

        protected virtual TEntity Create(TEntity model)
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }

        protected virtual void Delete(IQueryable<TEntity> query)
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }

        protected virtual void Delete(EntityKey<TEntity> key)
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }

        protected virtual IQueryable<TEntity> Read()
        {
            throw new ServiceException(CommunicationErrors.MethodIsNotAllowed, HttpStatusCode.MethodNotAllowed);
        }

        protected virtual IQueryable<TEntity> Read(EntityKey<TEntity> key)
        {
            return Read().Where(e => e != null).Where(key.MakePredicate())/*.Take(1)*/;
        }

        protected ApplicationEventSource Logger { get; private set; }

        #region Sealed overrides

        protected sealed override UpdatedODataResult<T> Updated<T>(T entity)
        {
            return base.Updated(entity);
        }

        protected sealed override BadRequestResult BadRequest()
        {
            return base.BadRequest();
        }

        protected sealed override InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            return base.BadRequest(modelState);
        }

        protected sealed override BadRequestErrorMessageResult BadRequest(string message)
        {
            return base.BadRequest(message);
        }

        protected sealed override ConflictResult Conflict()
        {
            return base.Conflict();
        }

        protected sealed override CreatedODataResult<T> Created<T>(T entity)
        {
            return base.Created(entity);
        }

        protected sealed override NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value)
        {
            return base.Content(statusCode, value);
        }

        protected sealed override FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
        {
            return base.Content(statusCode, value, formatter, mediaType);
        }

        protected sealed override CreatedNegotiatedContentResult<T> Created<T>(Uri location, T content)
        {
            return base.Created(location, content);
        }

        protected sealed override CreatedAtRouteNegotiatedContentResult<T> CreatedAtRoute<T>(string routeName, IDictionary<string, object> routeValues, T content)
        {
            return base.CreatedAtRoute(routeName, routeValues, content);
        }

        public sealed override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return base.ExecuteAsync(controllerContext, cancellationToken);
        }

        protected sealed override InternalServerErrorResult InternalServerError()
        {
            return base.InternalServerError();
        }

        protected sealed override ExceptionResult InternalServerError(Exception exception)
        {
            return base.InternalServerError(exception);
        }

        protected sealed override JsonResult<T> Json<T>(T content, JsonSerializerSettings serializerSettings, Encoding encoding)
        {
            return base.Json(content, serializerSettings, encoding);
        }

        protected sealed override NotFoundResult NotFound()
        {
            return base.NotFound();
        }

        protected sealed override OkResult Ok()
        {
            return base.Ok();
        }        

        protected sealed override OkNegotiatedContentResult<T> Ok<T>(T content)
        {
            return base.Ok(content);
        }

        protected sealed override RedirectResult Redirect(string location)
        {
            return base.Redirect(location);
        }

        protected sealed override RedirectResult Redirect(Uri location)
        {
            return base.Redirect(location);
        }

        protected sealed override RedirectToRouteResult RedirectToRoute(string routeName, IDictionary<string, object> routeValues)
        {
            return base.RedirectToRoute(routeName, routeValues);
        }

        protected sealed override ResponseMessageResult ResponseMessage(HttpResponseMessage response)
        {
            return base.ResponseMessage(response);
        }

        protected sealed override StatusCodeResult StatusCode(HttpStatusCode status)
        {
            return base.StatusCode(status);
        }

        protected sealed override UnauthorizedResult Unauthorized(IEnumerable<AuthenticationHeaderValue> challenges)
        {
            return base.Unauthorized(challenges);
        }

        #endregion
    }
}
