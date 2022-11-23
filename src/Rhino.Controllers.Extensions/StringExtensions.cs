/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 * https://stackoverflow.com/questions/18627112/how-can-i-convert-text-to-pascal-case
 */
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

// TODO: move entire class to Rhino.Api.Extensions or to Rhino.Parser and remove from here when available.
namespace Rhino.Controllers.Extensions
{
    /// <summary>
    /// Extension package for <see cref="string"/> object.
    /// </summary>
    [Obsolete("Migrated to Rhino.Api.Extensions and will be removed on future release.")]
    public static partial class StringExtensions
    {
        #region *** Expressions   ***
        [GeneratedRegex("(\\\\r\\\\n|\\\\n|\\\\r)")]
        private static partial Regex GetNewLineToken();

        [GeneratedRegex("\\W")]
        private static partial Regex GetNonWorkToken();
        #endregion

        #region *** Case Converts ***
        /// <summary>
        /// Normalize line breaks into Environment.NewLine format.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to normalize.</param>
        /// <returns>Normalized <see cref="string"/>.</returns>
        public static string NormalizeLineBreaks(this string str)
        {
            return GetNewLineToken().Replace(input: str, replacement: Environment.NewLine);
        }

        /// <summary>
        /// Converts a camelCase or PascalCase <see cref="string"/> to a snake_case <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to convert.</param>
        /// <returns>A snake_case <see cref="string"/>.</returns>
        public static string ToSnakeCase(this string str)
        {
            return InvokeSeparatorCase(str, '_').ToLower();
        }

        /// <summary>
        /// Converts a camelCase or PascalCase <see cref="string"/> to a kebab-case <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to convert.</param>
        /// <returns>A kebab-case <see cref="string"/>.</returns>
        public static string ToKebabCase(this string str)
        {
            return InvokeSeparatorCase(str, '-').ToLower();
        }

        /// <summary>
        /// Converts a camelCase or PascalCase <see cref="string"/> to a space case <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to convert.</param>
        /// <returns>A space case <see cref="string"/>.</returns>
        public static string ToSpaceCase(this string str)
        {
            return InvokeSeparatorCase(str, ' ');
        }

        private static string InvokeSeparatorCase(string str, char separator)
        {
            // collect the final result
            var result = new StringBuilder();

            // check and add chars (using for loop for performance)
            for (int i = 0; i < str.Length; i++)
            {
                var isZero = i == 0;
                var isLast = i == str.Length - 1;
                var isPrevious = !isZero && char.IsUpper(str[i - 1]);
                var isNext = !isLast && char.IsUpper(str[i + 1]);
                var isCurrent = char.IsUpper(str[i]);

                if (isZero || isLast || isCurrent && isNext && isPrevious || !isCurrent)
                {
                    result.Append(str[i]);
                    continue;
                }

                result.Append(separator);
                result.Append(str[i]);
            }

            // build the new string
            return result.ToString();
        }

        /// <summary>
        /// Converts <see cref="string"/> to PascalCase <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to convert.</param>
        /// <returns>A PascalCase <see cref="string"/>.</returns>
        public static string ToPascalCase(this string str)
        {
            // replace all non-letter and non-digits with an underscore and lowercase the rest.
            var result = string
                .Concat(str?.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_")
                .ToArray());

            // 1. split the resulting string by underscore
            // 2. select first character, uppercase it and concatenate with the rest of the string
            var arr = result?
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s[..1].ToUpper() + s[1..]);

            // join the resulting collection
            return string.Concat(arr);
        }
        #endregion

        /// <summary>
        /// Removes all non-word chars from the given <see cref="string"/>.
        /// </summary>
        /// <param name="input">The <see cref="string"/> to remove from.</param>
        /// <returns>The <see cref="string"/> without special chars.</returns>
        public static string RemoveNonWord(this string input)
        {
            return GetNonWorkToken().Replace(input, string.Empty);
        }

        /// <summary>
        /// Hash the <see cref="string"/> using SHA-3 algorithm.
        /// </summary>
        /// <returns>Hashed string.</returns>
        public static string Hash(this string str, string secretKey)
        {
            // setup
            var sha384 = new HMACSHA384(Encoding.UTF8.GetBytes(secretKey));
            var buffer = Encoding.UTF8.GetBytes(str);

            // build
            var hash = sha384.ComputeHash(buffer);
            var base64 = Encoding.UTF8.GetString(hash);

            // get
            return GetNonWorkToken().Replace(base64, string.Empty);
        }

        /// <summary>
        /// Converts a string into a <see cref="Stream" /> instance.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to convert.</param>
        /// <returns><see cref="Stream"/> object.</returns>
        public static Stream ToStream(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(str) ? "{}" : str);
            return new MemoryStream(bytes);
        }
    }
}
