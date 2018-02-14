using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.OData.Builder;

namespace BlackCoffeeTalk.Framework.Extensions
{
    public static class IEdmTypeExtensions
    {
        private static readonly LowerCamelCaser _caser = new LowerCamelCaser();

        public static Type GetClrType(this IEdmType etype)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(a => a.ExportedTypes).Where(t => !t.IsAbstract).FirstOrDefault(t => t.FullName == etype.ToString());
        }

        public static EntityKeyBase ToEntityKey(this KeySegment segment)
        {
           // var keyValueSegment = segment.Keys.First();
            var type = segment.EdmType.GetClrType();
            var entityKey = EntityKeyBase.From(type);
            foreach (var key in segment.Keys)
            {
                entityKey.Keys.Add(new KeyValuePair<string, object>(type.GetTypeInfo().GetProperties().First(p => _caser.ToLowerCamelCase(p.Name) == key.Key).Name, key.Value));
     
            }

            return entityKey;
        }
    }
}
