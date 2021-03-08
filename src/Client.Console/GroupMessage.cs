using System;

namespace GroupChat.Client.Console
{
    public class GroupMessage
    {
        public string Username { get; init; }
        public string Text { get; init; }
        public DateTime SentAt { get; init; }

        public GroupMessage()
        {
        }

        public GroupMessage(string username, string text, DateTime sentAt)
        {
            Username = username;
            Text = text;
            SentAt = sentAt;
        }

        public override string ToString() => $"{Username}: {Text}. {SentAt}";
    }
}