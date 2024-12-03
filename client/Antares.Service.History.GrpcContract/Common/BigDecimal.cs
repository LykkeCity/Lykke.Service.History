using System.Globalization;

namespace Antares.Service.History.GrpcContract.Common
{
    public partial class BigDecimal
    {
        public static implicit operator decimal(BigDecimal value)
        {
            return decimal.Parse(value.Value, CultureInfo.InvariantCulture);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            return new BigDecimal { Value = value.ToString(CultureInfo.InvariantCulture) };
        }
    }
}
