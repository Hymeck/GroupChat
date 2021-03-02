using System;

namespace GroupChat.Implementations.Dtos
{
    public class Message
    {
        public string Username { get; init; }
        public string ChatId { get; init; }
        public string Text { get; init; }
        public DateTime SentAt { get; init; }

        public Message()
        {
        }

        public Message(string username, string chatId, string text, DateTime sentAt)
        {
            Username = username;
            ChatId = chatId;
            Text = text;
            SentAt = sentAt;
        }

        public override string ToString() => $"{ChatId}. {Username}: {Text}. {SentAt}";
    }
}