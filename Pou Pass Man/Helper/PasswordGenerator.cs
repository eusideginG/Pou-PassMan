using System.Security.Cryptography;

namespace Pou_Pass_Man.Helper
{
    /// <summary>
    /// A class for generating passwords
    /// </summary>
    public static class PasswordGenerator
    {
        /// <summary>
        /// A static method that generates a password
        /// </summary>
        /// <param name="length">Represents the length of the password</param>
        /// <returns>A String password having latters numbers and symbols</returns>
        public static string Generate(int length)
        {
            int asciiStart = 33;
            int asciiEnd = 127;
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                result += ((char)RandomNumberGenerator.GetInt32(asciiStart, asciiEnd)).ToString();
            }
            return result;
        }
    }
}
