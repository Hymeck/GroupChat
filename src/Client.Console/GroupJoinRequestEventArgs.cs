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
        public IPAddress FromIpAddress { get; init; }

        public GroupJoinRequestEventArgs()
        {
        }

        public GroupJoinRequestEventArgs(string username, string groupId, DateTime requestedAt, IPAddress fromIpAddress)
        {
            Username = username;
            GroupId = groupId;
            RequestedAt = requestedAt;
            FromIpAddress = fromIpAddress;
        }

        public override string ToString() =>
            $"{GroupId}{Environment.NewLine}" +
            $"{Username}{Environment.NewLine}" +
            $"{FromIpAddress}{Environment.NewLine}" +
            $"{RequestedAt.ToString(CultureInfo.CurrentCulture)}";
    }
}