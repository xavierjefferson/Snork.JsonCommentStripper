using System.Text;
using System.Text.RegularExpressions;

namespace Snork.JsonCommentStripper
{
    static class StringExtensions
    {
        public static string Slice(this string input, int start, int end = int.MaxValue)
        {
            if (end == int.MaxValue)
                return input.Substring(start);
            return input.Substring(start, end - start);
        }
    }

    public class Stripper
    {
        private static string stripWithoutWhitespace(string _, int _a, int _b)
        {
            return string.Empty;
        }

        private static string StripWithWhitespace(string input, int start, int end)
        {
            return Regex.Replace(input.Slice(start, end),
                "[^\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]", " ");
        }

        private static bool isEscaped(string jsonString, int quotePosition)
        {
            var index = quotePosition - 1;
            var backslashCount = 0;

            while (jsonString[index] == '\\')
            {
                index -= 1;
                backslashCount += 1;
            }

            return backslashCount % 2 != 0;
        }

        public static string stripJsonComments(string input, bool whitespace = true,
            bool stripTrailingCommas = false)
        {
            var strip = whitespace ? (StripDelegate)StripWithWhitespace : stripWithoutWhitespace;

            var isInsideString = false;
            var commentStatus = CommentStatusEnum.None;
            var offset = 0;
            var buffer = new StringBuilder();
            var result = new StringBuilder();
            var commaIndex = -1;

            for (var index = 0; index < input.Length; index++)
            {
                var currentCharacter = input[index];
                string currentToken;
                if (index + 1 < input.Length)
                {
                    var nextCharacter = input[index + 1];
                    currentToken = string.Concat(currentCharacter, nextCharacter);
                }
                else
                {
                    currentToken = currentCharacter.ToString();
                }


                if (commentStatus == CommentStatusEnum.None && currentCharacter == '\"')
                {
                    // Enter or exit string
                    var escaped = isEscaped(input, index);
                    if (!escaped) isInsideString = !isInsideString;
                }

                if (isInsideString) continue;


                if (commentStatus == CommentStatusEnum.None && currentToken == "//")
                {
                    // Enter single-line comment
                    buffer.Append(input.Slice(offset, index));
                    offset = index;
                    commentStatus = CommentStatusEnum.SingleComment;
                    index++;
                }
                else if (commentStatus == CommentStatusEnum.SingleComment && currentToken == "\r\n")
                {
                    // Exit single-line comment via \r\n
                    index++;
                    commentStatus = CommentStatusEnum.None;
                    buffer.Append(strip(input, offset, index));
                    offset = index;
                }
                else if (commentStatus == CommentStatusEnum.SingleComment && currentCharacter == '\n')
                {
                    // Exit single-line comment via \n
                    commentStatus = CommentStatusEnum.None;
                    buffer.Append(strip(input, offset, index));
                    offset = index;
                }
                else if (commentStatus != CommentStatusEnum.None && currentToken == "/*")
                {
                    // Enter multiline comment
                    buffer.Append(input.Slice(offset, index));
                    offset = index;
                    commentStatus = CommentStatusEnum.MultiComment;
                    index++;
                }
                else if (commentStatus == CommentStatusEnum.MultiComment && currentToken == "*/")
                {
                    // Exit multiline comment
                    index++;
                    commentStatus = CommentStatusEnum.None;

                    buffer.Append(strip(input, offset, index + 1));
                    offset = index + 1;
                }
                else if (stripTrailingCommas && commentStatus == CommentStatusEnum.None)
                {
                    if (commaIndex != -1)
                    {
                        if (currentCharacter == '}' || currentCharacter == ']')
                        {
                            // Strip trailing comma
                            buffer.Append(input.Slice(offset, index));
                            var tmp = buffer.ToString();
                            result.Append(strip(tmp, 0, 1) + tmp.Slice(1));
                            buffer.Clear();
                            offset = index;
                            commaIndex = -1;
                        }
                        else if (currentCharacter != ' ' && currentCharacter != '\t' && currentCharacter != '\r' &&
                                 currentCharacter != '\n')
                        {
                            // Hit non-whitespace following a comma; comma is not trailing
                            buffer.Append(input.Slice(offset, index));
                            offset = index;
                            commaIndex = -1;
                        }
                    }
                    else if (currentCharacter == ',')
                    {
                        // Flush buffer prior to this point, and save new comma index
                        result.Append(buffer + input.Slice(offset, index));
                        buffer.Clear();
                        offset = index;
                        commaIndex = index;
                    }
                }
            }

            return string.Concat(result.ToString(), buffer.ToString(),
                commentStatus != CommentStatusEnum.None ? strip(input.Slice(offset)) : input.Slice(offset));
        }


        private enum CommentStatusEnum
        {
            SingleComment,
            MultiComment,
            None
        }

        private delegate string StripDelegate(string jsonString, int start = 0, int end = 0);
    }
}