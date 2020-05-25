using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;


namespace ScreenStreamer.Wpf.Common.Helpers
{
    public static class PropertySupport
    {
        private static readonly string expressionCannotBeNullMessage = "The expression cannot be null.";
        private static readonly string invalidExpressionMessage = "Invalid expression.";
        private static readonly string invalidPropertyName = "Invalid property name.";

        /// <summary>
        /// Gets value of the passed object's property by property name
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static TProperty GetPropertyValue<TObject, TProperty>(TObject obj, string propertyName)
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(obj.GetType()).Find(propertyName, false);
            if (propertyDescriptor == null)
                throw new ArgumentException(invalidPropertyName, propertyName);

            return (TProperty)propertyDescriptor.GetValue(obj);
        }

        /// <summary>
        /// Sets value of the passed object's property by property name
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue<TObject, TProperty>(TObject obj, string propertyName, TProperty value)
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(obj.GetType()).Find(propertyName, false);
            if (propertyDescriptor == null)
                throw new ArgumentException(invalidPropertyName, propertyName);

            propertyDescriptor.SetValue(obj, value);
        }

        /// <summary>
        /// Extracts object's property name from the passed expression
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ExtractPropertyName<TProperty>(Expression<Func<TProperty>> expression)
        {
            return GetMemberName(expression.Body);
        }

        private static string GetMemberName(System.Linq.Expressions.Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(expressionCannotBeNullMessage);
            }

            if (expression is MemberExpression)
            {
                // Reference type property or field
                var memberExpression = (MemberExpression)expression;
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression)
            {
                // Reference type method
                var methodCallExpression = (MethodCallExpression)expression;
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression)
            {
                // Property, field of method returning value type
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberName(unaryExpression);
            }

            throw new ArgumentException(invalidExpressionMessage);
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }

        public static class CompareHelper
        {
            public static bool AreEquals(object a, object b)
            {
                var enumerable = a as IEnumerable;
                if (enumerable != null && !(enumerable is string))
                    return AreEquals(enumerable.Cast<dynamic>(), (b as IEnumerable)?.Cast<dynamic>());

                return Equals(a, b);
            }

            public static bool AreEquals(IEnumerable<dynamic> a, IEnumerable<dynamic> b)
            {
                var aa = a?.ToArray();
                var bb = b?.ToArray();

                if (aa == null || bb == null)
                    return aa == bb;

                return aa.Length == bb.Length && (!aa.Except(bb).Any() || !bb.Except(aa).Any());
            }

            public static bool AreBinaryEquals<T>(T a, T b)
            {
                BinaryFormatter bf = new BinaryFormatter();
                byte[] a_result;
                byte[] b_result;

                // serialize a
                using (var stream = new MemoryStream())
                {
                    bf.Serialize(stream, a);
                    a_result = stream.ToArray();
                }
                // serialize b
                using (var stream = new MemoryStream())
                {
                    bf.Serialize(stream, b);
                    b_result = stream.ToArray();
                }
                // compare them
                return a_result.SequenceEqual(b_result);
            }
        }
    }
}
