using BlackCoffeeTalk.Framework.Common.Extentions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.ModelBinding;

namespace BlackCoffeeTalk.Framework.Extensions
{
    public static class ModelStateExtensions
    {
        public static void SkipIfContainsNestedProperties<T, TReturn>(this ModelStateDictionary modelState, Expression<Func<T, TReturn>> expression)
        {
            var propName = GenericExtensions.GetPropertyName(expression);
            modelState
                .Keys
                .Where(x => x.Contains(propName) && !x.Equals(propName))
                .ToList()
                .ForEach(key => modelState[key].Errors.Clear());
        }
    }
}
