using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace BlackCoffeeTalk.Framework
{
    public abstract class EntityKeyBase
    {
        public ObservableCollection<KeyValuePair<string, object>> Keys { get; private set; } = new ObservableCollection<KeyValuePair<string, object>>();

        public static EntityKeyBase From(Type type)
        {
            var keytype = typeof(EntityKey<>).MakeGenericType(type);
            var key = (EntityKeyBase)Activator.CreateInstance(keytype);
            key.Keys = new ObservableCollection<KeyValuePair<string, object>>();
            return key;
        }
    }

    [ModelBinder(typeof(EntityKeyModelBinder))]
    public class EntityKey<TEntity> : EntityKeyBase
    {
        private Expression<Func<TEntity, bool>> _predicate = null;

        public EntityKey()
        {
            Keys.CollectionChanged += (s, e) => _predicate = null;
        }

        public Expression<Func<TEntity, bool>> MakePredicate()
        {
            if (_predicate == null)
            {
                var param = Expression.Parameter(typeof(TEntity), "e");
                Expression prev = null;
                foreach (KeyValuePair<string, object> kvp in Keys)
                {
                    var member = Expression.PropertyOrField(param, kvp.Key);
                    var value = Expression.Convert(Expression.Constant(kvp.Value), member.Type);
                    Expression curr = Expression.Equal(member, value);
                    if (prev == null)
                    {
                        prev = curr;
                    }
                    else
                    {
                        prev = Expression.AndAlso(prev, curr);
                    }
                }

                _predicate = Expression.Lambda<Func<TEntity, bool>>(prev, param);
            }

            return _predicate;
        }

        public bool IsMatch(TEntity entity)
        {
            return MakePredicate().Compile()(entity);
        }
    }

    internal class EntityKeyModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType.BaseType == typeof(EntityKeyBase))
            {
                bindingContext.Model = actionContext.RequestContext.RouteData.Values["selector"];
                return true;
            }

            return false;
        }
    }
}
