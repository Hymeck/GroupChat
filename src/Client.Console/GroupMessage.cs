using System;

namespace GroupChat.Client.Console
{
    public class GroupMessage
    {
        public string Username { get; init; }
        // public string GroupId { get; init; }
        public string Text { get; init; }
        public DateTime SentAt { get; init; }

        public GroupMessage()
        {
        }

        // public Message(string username, string groupId, string text, DateTime sentAt)
        public GroupMessage(string username, string text, DateTime sentAt)
        {
            Username = username;
            // GroupId = groupId;
            Text = text;
            SentAt = sentAt;
        }

        // public override string ToString() => $"{GroupId}. {Username}: {Text}. {SentAt}";
        public override string ToString() => $"{Username}: {Text}. {SentAt}";
    }
}