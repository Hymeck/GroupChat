using System;
using System.Globalization;
using System.Net;

namespace GroupChat.Client.Console
{
    public class GroupJoinRequestEventArgs : EventArgs
    {
        public string Username { get; init; }
        public string GroupId { get; init; }
        public DateTime RequestedAt { get; init; }
        public IPEndPoint From { get; init; }

        public GroupJoinRequestEventArgs()
        {
        }

        public GroupJoinRequestEventArgs(string username, string groupId, DateTime requestedAt, IPEndPoint from)
        {
            Username = username;
            GroupId = groupId;
            RequestedAt = requestedAt;
            From = from;
        }

        public override string ToString() =>
            $"{GroupId}{Environment.NewLine}" +
            $"{Username}{Environment.NewLine}" +
            $"{From}{Environment.NewLine}" +
            $"{RequestedAt.ToString(CultureInfo.CurrentCulture)}";
    }
}