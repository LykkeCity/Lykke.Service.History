using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Antares.Service.History.PostgresRepositories.JsonbQuery
{
    internal class NpgsqlFragmentExpression : SqlFragmentExpression
    {
        /// <inheritdoc />
        /// <summary>
        ///     Creates a new instance of a NpgsqlFragmentExpression.
        /// </summary>
        public NpgsqlFragmentExpression([NotNull] string sql, Type type)
            : base(sql)
        {
            Type = type;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.
        ///     (Inherited from <see cref="T:System.Linq.Expressions.Expression" />.)
        /// </summary>
        /// <returns>
        ///     The <see cref="P:Microsoft.EntityFrameworkCore.Query.Expressions.SqlFragmentExpression.Type" /> that
        ///     represents the static type of the expression.
        /// </returns>
        public override Type Type { get; }
    }
}
