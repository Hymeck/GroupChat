using System;

namespace Entry
{
    public class Message
    {
        public string Username { get; init; }
        public string ChatId { get; init; }
        public string Text { get; init; }
        public DateTimeOffset SentAt { get; init; }

        public Message()
        {
        }

        public Message(string username, string chatId, string text, DateTimeOffset sentAt)
        {
            Username = username;
            ChatId = chatId;
            Text = text;
            SentAt = sentAt;
        }

        public override string ToString() => $"{ChatId}. {Username}: {Text}. {SentAt}";
    }
}