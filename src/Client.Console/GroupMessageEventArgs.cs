using System;
using System.Net;

namespace GroupChat.Client.Console
{
    public class GroupMessageEventArgs : EventArgs
    {
        public string Username { get; init; }
        public string GroupId { get; init; }
        public string Text { get; init; }
        public DateTime SentAt { get; init; }
        public IPAddress From { get; init; }

        public GroupMessageEventArgs()
        {
        }

        public GroupMessageEventArgs(string username, string groupId, string text, DateTime sentAt, IPAddress from)
        {
            Username = username;
            GroupId = groupId;
            Text = text;
            SentAt = sentAt;
            From = from;
        }

        public override string ToString() => $"{Username} {GroupId} {Text} {From} {SentAt}";
    }
}