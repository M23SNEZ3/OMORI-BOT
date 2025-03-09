using System.Net;

namespace OMORI_BOT.M23.Extensions
{

    public static class StringExtensions
    {
        /// <summary>
        /// Provides extension methods for string manipulation.
        /// </summary>
        public static string EncodeHeader(this string data)
        {
            return WebUtility.UrlEncode(data);
        }
    }
}