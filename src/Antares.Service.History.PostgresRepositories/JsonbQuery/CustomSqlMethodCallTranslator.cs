using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Antares.Service.History.PostgresRepositories.JsonbQuery
{
    internal class CustomSqlMethodCallTranslator : NpgsqlCompositeMethodCallTranslator
    {
        public CustomSqlMethodCallTranslator(RelationalCompositeMethodCallTranslatorDependencies dependencies,
            INpgsqlOptions npgsqlOptions) : base(dependencies, npgsqlOptions)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            AddTranslators(new[] {new JsonbFindTranslator()});
        }
    }
}
