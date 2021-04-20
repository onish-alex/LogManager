using LogManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager.Core.Utilities
{
    public class IpAddressComparerReverse : IComparer<Ip>
    {
        public int Compare(Ip x, Ip y)
        {
            return StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, false)
                                                 .Compare(
                                IpConverter.FromBytes(y.Address),
                                IpConverter.FromBytes(x.Address));
        }
    }
}
