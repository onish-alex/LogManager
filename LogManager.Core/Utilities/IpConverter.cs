using LogManager.Core.Enumerations;
using LogManager.Core.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogManager.Core.Utilities
{
    public static class IpConverter
    {
        public static string FromBytes(byte[] ip)
        {
            switch (ip.Length)
            {
                case (int)IpLength.V4:
                    return string.Join('.', ip);

                case (int)IpLength.V6:
                    {
                        var builder = new StringBuilder();
                    
                        for (var i = 0; i < ip.Length; i += 2)
                        {
                            builder.AppendFormat("{0}{1}:", ip[i].ToString("x2"), ip[i + 1].ToString("x2"));
                        }
                    
                        builder.Remove(builder.Length - 1, 1);
                        return builder.ToString();
                    }

                default: throw new ArgumentException(ErrorMessages.InvalidIpFormat);
            }
        }

        public static byte[] FromString(string ip)
        {
            var ipInBytes = new List<byte>();

            if (ip.Contains((char)IpDelimeters.V4))
            {
                var ipStrParts = ip.Split(
                    (char)IpDelimeters.V4, 
                    StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < ipStrParts.Length; i++)
                {
                    if (byte.TryParse(ipStrParts[i], out byte readByte))
                    {
                        ipInBytes.Add(readByte);
                    }
                    else
                    {
                        throw new ArgumentException(ErrorMessages.InvalidIpFormat);
                    }
                    
                }
            }
            else if (ip.Contains((char)IpDelimeters.V6))
            {
                var ipStrParts = ip.Split(
                    (char)IpDelimeters.V6, 
                    StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < ipStrParts.Length; i++)
                {
                    if (short.TryParse(ipStrParts[i], 
                        System.Globalization.NumberStyles.HexNumber, 
                        null, 
                        out short bytePair))
                    {
                        ipInBytes.Add((byte)(bytePair >> 8));
                        ipInBytes.Add((byte)(bytePair & byte.MaxValue));
                    }
                    else
                    {
                        throw new ArgumentException(ErrorMessages.InvalidIpFormat);
                    }
                }
            }
            else
            {
                throw new ArgumentException(ErrorMessages.InvalidIpFormat);
            }

            return ipInBytes.ToArray();
        }

        public static long ToLong(byte[] ip)
        {
            var result = 0L;
            var multiplier = 1;

            for (int i = ip.Length - 1; i >= 0; i--)
            {
                result += (long)ip[i] * multiplier;
                multiplier *= 1000;
            }

            return result;
        }
    }
}
