using System;

namespace GroupChat.Client
{
    public class GroupJoinRequest
    {
        public string Username { get; init; }
        public string GroupId { get; init; }
        public DateTime SentAt { get; init; }

        public GroupJoinRequest()
        {
        }

        public GroupJoinRequest(string username, string groupId, DateTime sentAt)
        {
            Username = username;
            GroupId = groupId;
            SentAt = sentAt;
        }
    }
}