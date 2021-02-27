using System;

namespace Entry
{
    /// <summary>
    /// Represents data request to join a group.
    /// </summary>
    public class GroupAccessRequest
    {
        public string ChatId { get; init; }
        public int UsernameLength { get; init; }
        public string Username { get; init; }

        public GroupAccessRequest()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of <see cref="GroupAccessRequest"/> class with specified <paramref name="chatId"/> and username.
        /// </summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="username">Username.</param>
        /// <exception cref="NullReferenceException"><paramref name="username"/> or <paramref name="chatId"/> is null.</exception>
        // /// <exception cref="FormatException"><paramref name="chatId"/> is not a GUID string.</exception>
        public GroupAccessRequest(string chatId, string username)
        {
            ChatId = chatId ?? throw new NullReferenceException(nameof(chatId));
            Username = username ?? throw new NullReferenceException(nameof(username));
            UsernameLength = username.Length;
        }

        public override string ToString() => $"{Username}:{ChatId}";
    }
}