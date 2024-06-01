using System.Text;
using System.Text.RegularExpressions;
using Snork.JsonCommentStripper.Enums;
using Snork.JsonCommentStripper.Model;

namespace Snork.JsonCommentStripper
{
    public static class Stripper
    {
        private static readonly Regex NonWhiteSpaceRegex = new Regex("\\S", RegexOptions.Compiled);

        private static bool IsEscaped(string input, int quotePosition)
        {
            if (quotePosition == 0) return false;
            var index = quotePosition - 1;
            var backslashCount = 0;

            while (input[index] == '\\')
            {
                index -= 1;
                backslashCount += 1;
            }

            return backslashCount % 2 != 0;
        }

        public static string Execute(string input, StripperOptions? stripperOptions = null)
        {
            stripperOptions ??= new StripperOptions { StripTrailingCommas = false, ReplaceWithWhiteSpace = true };

            var executionInfo = new ExecutionInfo
            {
                Input = input,
                StripFunc = stripperOptions.ReplaceWithWhiteSpace
                    ? (CommentStripperDelegate)((input1, start, end) =>
                    {
                        var tmp = NonWhiteSpaceRegex.Replace(input1.Slice(start, end), " ");
                        return tmp;
                    })
                    : (x, y, z) => string.Empty,
                StringMode = StringModeEnum.None,
                CommentMode = CommentModeEnum.Default,
                Offset = 0,
                Buffer = new StringBuilder(),
                Result = new StringBuilder(),
                CommaIndex = -1,
                Options = stripperOptions
            };

            for (executionInfo.Index = 0; executionInfo.Index < executionInfo.Input.Length; executionInfo.Index++)
            {
                executionInfo.CurrentCharacter = executionInfo.Input[executionInfo.Index];
                if (executionInfo.Index + 1 < executionInfo.Input.Length)
                {
                    var nextCharacter = executionInfo.Input[executionInfo.Index + 1];
                    executionInfo.CurrentToken = string.Concat(executionInfo.CurrentCharacter, nextCharacter);
                }
                else
                {
                    executionInfo.CurrentToken = executionInfo.CurrentCharacter.ToString();
                }


                if (executionInfo.CommentMode == CommentModeEnum.Default)
                {
                    StringModeEnum? flipMode = null;
                    switch (executionInfo.CurrentCharacter)
                    {
                        case '\"':
                            if (executionInfo.StringMode != StringModeEnum.SingleQuote)
                                flipMode = StringModeEnum.DoubleQuote;
                            break;

                        case '\'':
                            if (executionInfo.StringMode != StringModeEnum.DoubleQuote)
                                flipMode = StringModeEnum.SingleQuote;
                            break;
                    }

                    if (flipMode != null)
                    {
                        // Enter or exit string
                        var escaped = IsEscaped(executionInfo.Input, executionInfo.Index);

                        if (!escaped)
                        {
                            if (executionInfo.StringMode == flipMode.Value)
                                executionInfo.StringMode = StringModeEnum.None;
                            else
                                executionInfo.StringMode = flipMode.Value;
                        }
                    }
                }

                if (executionInfo.StringMode != StringModeEnum.None) continue;

                switch (executionInfo.CommentMode)
                {
                    case CommentModeEnum.Default:
                        HandleDefaultMode(executionInfo);
                        break;
                    case CommentModeEnum.SingleComment:
                        HandleSingleCommentMode(executionInfo);

                        break;
                    case CommentModeEnum.MultiComment:
                        HandleMultiCommentMode(executionInfo);

                        break;
                }
            }

            var remainder = executionInfo.Input.Slice(executionInfo.Offset);
            return string.Concat(executionInfo.Result, executionInfo.Buffer,
                executionInfo.CommentMode == CommentModeEnum.Default
                    ? remainder
                    : executionInfo.StripFunc(remainder));
        }

        private static void HandleDefaultMode(ExecutionInfo executionInfo)
        {
            switch (executionInfo.CurrentToken)
            {
                case "//":
                    // Enter single-line comment
                    executionInfo.Buffer.Append(executionInfo.Input.Slice(executionInfo.Offset, executionInfo.Index));
                    executionInfo.Offset = executionInfo.Index;
                    executionInfo.CommentMode = CommentModeEnum.SingleComment;
                    executionInfo.Index++;
                    break;
                case "/*":
                    // Enter multiline comment
                    executionInfo.Buffer.Append(executionInfo.Input.Slice(executionInfo.Offset, executionInfo.Index));
                    executionInfo.Offset = executionInfo.Index;
                    executionInfo.CommentMode = CommentModeEnum.MultiComment;
                    executionInfo.Index++;
                    break;
                default:

                    if (!executionInfo.Options.StripTrailingCommas) return;

                    if (executionInfo.CommaIndex != -1)
                    {
                        switch (executionInfo.CurrentCharacter)
                        {
                            case '}':
                            case ']': // Strip trailing comma
                                executionInfo.Buffer.Append(
                                    executionInfo.Input.Slice(executionInfo.Offset, executionInfo.Index));
                                var tmp = executionInfo.Buffer.ToString();
                                executionInfo.Result.Append(executionInfo.StripFunc(tmp, 0, 1));
                                executionInfo.Result.Append(tmp.Slice(1));
                                executionInfo.Buffer.Clear();
                                executionInfo.Offset = executionInfo.Index;
                                executionInfo.CommaIndex = -1;
                                break;
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                //skip these whitespace characters
                                break;
                            default:
                                // Hit non-whitespace following a comma; comma is not trailing
                                executionInfo.Buffer.Append(
                                    executionInfo.Input.Slice(executionInfo.Offset, executionInfo.Index));
                                executionInfo.Offset = executionInfo.Index;
                                executionInfo.CommaIndex = -1;
                                break;
                        }
                    }
                    else if (executionInfo.CurrentCharacter == ',')
                    {
                        // Flush status.buffer prior to this point, and save new comma status.index
                        executionInfo.Result.Append(string.Concat(executionInfo.Buffer,
                            executionInfo.Input.Slice(executionInfo.Offset, executionInfo.Index)));
                        executionInfo.Buffer.Clear();
                        executionInfo.Offset = executionInfo.Index;
                        executionInfo.CommaIndex = executionInfo.Index;
                    }


                    break;
            }
        }

        private static void HandleMultiCommentMode(ExecutionInfo executionInfo)
        {
            if (executionInfo.CurrentToken == "*/")
            {
                // Exit multiline comment
                executionInfo.Index++;
                executionInfo.CommentMode = CommentModeEnum.Default;

                executionInfo.Buffer.Append(executionInfo.StripFunc(executionInfo.Input, executionInfo.Offset,
                    executionInfo.Index + 1));
                executionInfo.Offset = executionInfo.Index + 1;
            }
        }

        private static void HandleSingleCommentMode(ExecutionInfo executionInfo)
        {
            var endLine = false;
            var increment = 0;
            if (executionInfo.CurrentToken == "\r\n")
            {
                // Exit single-line comment via \r\n
                increment = 1;
                endLine = true;
            }
            else if (executionInfo.CurrentCharacter == '\n')
            {
                // Exit single-line comment via \n
                endLine = true;
            }

            if (endLine)
            {
                executionInfo.Index += increment;
                executionInfo.CommentMode = CommentModeEnum.Default;
                executionInfo.Buffer.Append(executionInfo.StripFunc(executionInfo.Input, executionInfo.Offset,
                    executionInfo.Index));
                executionInfo.Offset = executionInfo.Index;
            }
        }
    }
}