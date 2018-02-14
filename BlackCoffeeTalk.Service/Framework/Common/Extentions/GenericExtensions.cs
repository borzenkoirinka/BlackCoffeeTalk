using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace BlackCoffeeTalk.Framework.Common.Extentions
{
    /// <summary>
    /// Extensions for exception class.
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Get last inner exception.
        /// </summary>
        /// <param name="exception">Exception from where gets last InnerException</param>
        /// <returns>Return last inner exception.</returns>
        public static Exception GetLastInnerException(this Exception exception)
        {
            return exception.InnerException != null ? exception.InnerException.GetLastInnerException() : exception;
        }

        /// <summary>
        /// Get last inner exception with stack trace.
        /// </summary>
        /// <param name="exception"> Exception where gets last InnerException. </param>
        /// <returns>Return last inner exception  with stack trace. </returns>
        public static Exception GetLastInnerExceptionWithStackTrace(this Exception exception)
        {
            return exception.InnerException != null && !string.IsNullOrEmpty(exception.InnerException.StackTrace) ? exception.InnerException.GetLastInnerExceptionWithStackTrace() : exception;
        }

        public static string FingMessagException(this Exception exception, string message)
        {
            return exception.InnerException?.FingMessagException(message) ?? (exception.Message.ToLower().Contains(message.ToLower()) ? exception.Message : null);
        }


        public static TReturn GetAttribute<TReturn>(object sourse) where TReturn : Attribute
        {
            return sourse
                    .GetType()
                    .GetCustomAttributes(typeof(TReturn), false)
                    .Cast<TReturn>()
                    .SingleOrDefault();
        }


        /// <summary>
        /// Get specific exception.
        /// </summary>
        /// <param name="exception">Exception from where gets specific exception</param>
        /// <returns>Return specific exception.</returns>
        public static TException GetSpecificException<TException>(this Exception exception) where TException : Exception
        {
            if (exception.InnerException != null)
                return exception.InnerException is TException
                    ? (TException)exception.InnerException
                    : exception.InnerException.GetSpecificException<TException>();
            return null;
        }

        public static string GetPropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }

        public static string Description<T>(this T source)
        {
            var l_descriptionAttribute = source.GetAttribute<DescriptionAttribute, T>();
            if (l_descriptionAttribute != null)
            {
                return l_descriptionAttribute.Description;
            }

            return source.ToString();
        }
        public static TReturn GetAttribute<TReturn, TSourse>(this TSourse sourse) where TReturn : Attribute
        {
            return sourse
                .GetType()
                .GetMember(sourse.ToString())[0]
                .GetCustomAttributes(typeof(TReturn), false)
                .Cast<TReturn>()
                .SingleOrDefault();
        }
    }
    
}
