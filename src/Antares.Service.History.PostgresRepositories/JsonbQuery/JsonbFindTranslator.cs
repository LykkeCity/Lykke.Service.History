using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Antares.Service.History.PostgresRepositories.JsonbQuery
{
    internal class JsonbFindTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo MethodInfo
            = typeof(JsonbExtensions).GetRuntimeMethod(nameof(JsonbExtensions.JsonbPath),
                new[] {typeof(string), typeof(string)});

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.GetGenericMethodDefinition() != MethodInfo) return null;

            var objectExpression = (ColumnExpression) methodCallExpression.Arguments[0];
            var jsonbKeyName = (ConstantExpression) methodCallExpression.Arguments[1];

            var sqlExpression = new NpgsqlFragmentExpression($"{objectExpression}->>'{jsonbKeyName.Value}'",
                methodCallExpression.Method.ReturnType);

            return sqlExpression;
        }
    }
}
