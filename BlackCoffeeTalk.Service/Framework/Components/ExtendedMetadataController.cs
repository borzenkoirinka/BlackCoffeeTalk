using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using System;
using System.Reflection;
using System.Web.OData.Builder;
using BlackCoffeeTalk.Framework.Extensions;
using System.Collections;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using BlackCoffeeTalk.Framework.Common;

namespace BlackCoffeeTalk.Framework.Components
{
    public class ValidationEntityDocument
    {
        [Key]
        public string Entity { get; set; }
        public List<ValidationPropertyDocument> Properties { get; set; }
    }

    public class ValidationPropertyDocument
    {
        public string Name { get; set; }
        public List<ValidationValidatorDocument> Validators { get; set; }
    }

    public class ValidationValidatorDocument
    {
        public string Scheme { get; set; }
        public List<string> Parameters { get; set; }
    }

    public class ExtendedMetadataController : ODataController
    {
        private readonly IEdmModel _model;
        private readonly LowerCamelCaser _caser = new LowerCamelCaser();

        public ExtendedMetadataController(IEdmModel model)
        {
            _model = model;
        }

        [HttpGet]
        [ODataRoute(Constants.ValidationFunctionName)]
        [ActionName(Constants.ValidationFunctionName)]
        [EnableQuery]
        public IHttpActionResult GetValidationDocument()
        {
            var doc = new List<ValidationEntityDocument>();

            foreach (var entity in _model.EntityContainer.EntitySets())
            {
                doc.Add(new ValidationEntityDocument
                {
                    Entity = entity.Name,
                    Properties = GetPropertiesValidation(entity)
                });
            }


            return Ok(doc.AsQueryable());
        }

        private List<ValidationPropertyDocument> GetPropertiesValidation(IEdmEntitySet entity)
        {
            var properties = new List<ValidationPropertyDocument>();

            var entityType = entity.Type is IEdmCollectionType ?
                 (EdmEntityType)((IEdmCollectionType)entity.Type).ElementType.Definition :
                 (EdmEntityType)entity.Type;

            var type = entityType.GetClrType();

            foreach (var prop in entityType.DeclaredProperties)
            {
                var validators = GetValidators(GetMatchedClrProperty(prop, type));

                if (validators.Any())
                    properties.Add(new ValidationPropertyDocument
                    {
                        Name = prop.Name,
                        Validators = validators
                    });
            }


            return properties;
        }

        private List<ValidationValidatorDocument> GetValidators(PropertyInfo propertyInfo)
        {
            var validations = new List<ValidationValidatorDocument>();
            if (propertyInfo != null)
            {
                var attrs = propertyInfo.GetCustomAttributesData().Where(ca => typeof(ValidationAttribute).IsAssignableFrom(ca.AttributeType));

                foreach (var attr in attrs)
                {
                    var paramz = attr.ConstructorArguments.Select((ca, i) => $"{_caser.ToLowerCamelCase(attr.Constructor.GetParameters()[i].Name)}={GetFieldValue(ca.Value)}")
                        .Concat(attr.NamedArguments.Select(na => $"{_caser.ToLowerCamelCase(na.MemberName)}={GetFieldValue(na.TypedValue.Value)}")).ToList();

                    validations.Add(new ValidationValidatorDocument
                    {
                        Scheme = _caser.ToLowerCamelCase(attr.AttributeType.Name.Replace("Attribute", "")),
                        Parameters = paramz
                    });
                }
            }
            return validations;
        }

        private PropertyInfo GetMatchedClrProperty(IEdmProperty prop, Type type)
        {
            return type.GetProperties().FirstOrDefault(p => _caser.ToLowerCamelCase(p.Name) == prop.Name);
        }

        private string GetFieldValue(object value)
        {
            if (value == null)
                return "null";

            if(value is ReadOnlyCollection<CustomAttributeTypedArgument>)
            {
                value = ((ReadOnlyCollection<CustomAttributeTypedArgument>)value).Select(v => v.Value).ToArray();
                return GetFieldValue(value);
            }
            if (value is Type && typeof(Enum).IsAssignableFrom(value as Type))
            {
                return JsonConvert.SerializeObject(Enum.GetNames(value as Type));
            }
            return JsonConvert.SerializeObject(value);
        }
    }
}
