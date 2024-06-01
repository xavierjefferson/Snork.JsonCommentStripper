using System.Text;
using Snork.JsonCommentStripper.Enums;

namespace Snork.JsonCommentStripper.Model
{
    internal class ExecutionInfo
    {
        public CommentModeEnum CommentMode { get; set; }
        public int Offset { get; set; }
        public StringBuilder? Buffer { get; set; }
        public StringBuilder? Result { get; set; }
        public int CommaIndex { get; set; }
        public StringModeEnum StringMode { get; set; }
        public CommentStripperDelegate? StripFunc { get; set; }
        public int Index { get; set; }
        public string? CurrentToken { get; set; }
        public char CurrentCharacter { get; set; }
        public string? Input { get; set; }
        public StripperOptions? Options { get; set; }
    }
}