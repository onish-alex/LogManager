namespace LogManager.Web.Utilities
{
    public static class ViewHelper
    {
        public static string GetSortArrow(bool isDescending)
        {
            if (isDescending)
            {
                return "&#9660;";
            }
            else
            {
                return "&#9650;";
            }
        }
    }
}
