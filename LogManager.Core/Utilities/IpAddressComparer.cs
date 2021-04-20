using LogManager.Core.Entities;
using System;
using System.Collections.Generic;

namespace LogManager.Core.Utilities
{
    public class IpAddressComparer : IComparer<Ip>
    {
        public int Compare(Ip x, Ip y)
        {
            return StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, false)
                                                 .Compare(
                                IpConverter.FromBytes(x.Address),
                                IpConverter.FromBytes(y.Address));
        }
    }
}
