#nullable enable
using System;

namespace GroupChat.Client.Console
{
    public sealed class GroupJoinDeniedException : Exception
    {
        public GroupJoinDeniedException() : base("Group join request was denied.")
        {
        }

        public GroupJoinDeniedException(string? message) : base(message)
        {
        }
    }
}