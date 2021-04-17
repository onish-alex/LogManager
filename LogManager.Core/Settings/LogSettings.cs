namespace LogManager.Core.Settings
{
    public static class LogSettings
    {
        public static readonly int EntryPartsCount = 15;

        public static readonly string IpV4Pattern = "^((25[0-5]|(2[0-4]|1[0-9]|[1-9]|)[0-9])(\\.(?!$)|$)){4}$";

        public static readonly string IpV6Pattern = "([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}";

        public static readonly string DateTimeOffsetPattern = "[dd/MMM/yyyy:HH:mm:ss zzzz]";
    }
}
