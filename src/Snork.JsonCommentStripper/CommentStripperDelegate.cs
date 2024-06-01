namespace Snork.JsonCommentStripper
{
    internal delegate string? CommentStripperDelegate(string input, int start = 0, int? end = null);
}