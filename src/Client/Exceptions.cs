#nullable enable
using System;

namespace GroupChat.Client
{
    public sealed class GroupJoinRejectedException : Exception
    {
        public GroupJoinRejectedException() : base("Group join request was rejected.")
        {
        }

        public GroupJoinRejectedException(string? message) : base(message)
        {
        }
    }
}