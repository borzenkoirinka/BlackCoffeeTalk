using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Microsoft.OData.Edm;
using Newtonsoft.Json;

namespace BlackCoffeeTalk.Framework.Infrastructure
{
    public class ExtendedFilterBinder : FilterBinder
    {
        public ExtendedFilterBinder(IServiceProvider requestContainer) : base(requestContainer)
        {
        }

        public override Expression Bind(QueryNode node)
        {
            return base.Bind(node);
        }                

        public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            if (node.Name == "containsany")
                return BindContainsAny(node);
            if (node.Name == "incollection")
                return BindInCollection(node);
            if (node.Name == "notincollection")
                return BindNotInCollection(node);

            var exp =  base.BindSingleValueFunctionCallNode(node);

            return exp;
        }

        private Expression BindInCollection(SingleValueFunctionCallNode node)
        {
            var argument = Bind(node.Parameters.First());

            var collection = ((IEnumerable)JsonConvert.DeserializeObject(node.Parameters.OfType<ConstantNode>().First().LiteralText,
                typeof(IEnumerable<>).MakeGenericType(argument.Type))).AsQueryable();

            var data = Expression.Constant(collection);

            var result = Expression.Call(typeof(Queryable), "Contains", new Type[] { argument.Type }, data, argument);

            return result;
        }

        private Expression BindNotInCollection(SingleValueFunctionCallNode node)
        {
            var result = Expression.Not(BindInCollection(node));
            return result;
        }

        private Expression BindAnyCollection(SingleValueFunctionCallNode node)
        {
            var argument = Bind(node.Parameters.First());

            var collection = ((IEnumerable)JsonConvert.DeserializeObject(node.Parameters.OfType<ConstantNode>().First().LiteralText,
                typeof(IEnumerable<>).MakeGenericType(argument.Type))).AsQueryable();

            var data = Expression.Constant(collection);

            var anyParam = Expression.Parameter(argument.Type);
            var exp = Expression.MakeBinary(ExpressionType.Equal, argument, anyParam);

            var lambda = Expression.Lambda(exp, anyParam);

            var result = Expression.Call(typeof(Queryable), "Any", new Type[] { argument.Type }, data, lambda);

            return result;
        }
        private Expression BindContainsAny(SingleValueFunctionCallNode node)
        {
            var argument = Bind(node.Parameters.First());
            
            var collection = ((IEnumerable)JsonConvert.DeserializeObject(node.Parameters.OfType<ConstantNode>().First().LiteralText,
                typeof(IEnumerable<>).MakeGenericType(argument.Type))).AsQueryable();

            var data = Expression.Constant(collection);
            var anyParam = Expression.Parameter(argument.Type);
            var anyFunc = Expression.Lambda(Expression.Call(argument, "Contains", new Type[] { }, anyParam), anyParam);
            var result = Expression.Call(typeof(Queryable), "Any", new Type[] { argument.Type }, data, anyFunc);

            return result;
        }

        public static void RegisterCustomFunctions()
        {
            var signature = new[]
            {
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), new IEdmTypeReference[]
                {
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                }),
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), new IEdmTypeReference[]
                {
                    EdmCoreModel.Instance.GetInt32(true),
                    EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetInt32(false))
                }),
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), new IEdmTypeReference[]
                {
                    EdmCoreModel.Instance.GetInt32(true),
                    EdmCoreModel.Instance.GetString(true)
                    //new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetString(true)))                
                })
            };
            foreach (var sign in signature)
            {
                CustomUriFunctions.AddCustomUriFunction("incollection", sign);
                CustomUriFunctions.AddCustomUriFunction("notincollection", sign);

            }

            CustomUriFunctions.AddCustomUriFunction("containsany", new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), new IEdmTypeReference[]
            {
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
            }));
        }
    }
}
