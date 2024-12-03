using System;

namespace Antares.Service.History.PostgresRepositories.JsonbQuery
{
    internal static class JsonbExtensions
    {
        public static T JsonbPath<T>(this string jsonb, string property)
        {
            throw new NotImplementedException("This method is not supposed to run on client");
        }
    }
}
