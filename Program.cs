namespace Exncod
{
    using System;
    using System.Text;

    /// <summary>The program.</summary>
    public class Program
    {
        /// <summary>The main.</summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine(UrlEncode("https://acme.com/how to perform fast url decode?yo=rick!"));
        }

        /// <summary>The url encode.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string UrlEncode(string value)
        {
            if (value == null)
            {
                return null;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length, alwaysCreateNewReturnValue: false));
        }

        /// <summary>The url encode.</summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="alwaysCreateNewReturnValue">The always create new return value.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        /// <summary>The url encode.</summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int charSpaces = 0;
            int charUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                {
                    charSpaces++;
                }
                else if (!IsUrlSafeChar(ch))
                {
                    charUnsafe++;
                }
            }

            // nothing to expand?
            if (charSpaces == 0 && charUnsafe == 0)
            {
                if (0 == offset && bytes.Length == count)
                {
                    return bytes;
                }

                var subArray = new byte[count];
                Buffer.BlockCopy(bytes, offset, subArray, 0, count);
                return subArray;
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + (charUnsafe * 2)];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        /// <summary>The validate url encoding parameters.</summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if offset is null.</exception>
        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
            {
                return false;
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return true;
        }

        /// <summary>The Integer to hex.</summary>
        /// <param name="n">The number.</param>
        /// <returns>The <see cref="char"/>.</returns>
        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + (int)'0');
            }

            return (char)(n - 10 + (int)'A');
        }

        /// <summary>
        /// Set of safe chars, from RFC 1738.4 minus '+'
        /// </summary>
        /// <param name="ch">The char.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool IsUrlSafeChar(char ch)
        {
            if ((ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z')
                || (ch >= '0' && ch <= '9'))
            {
                return true;
            }

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }
    }
}
