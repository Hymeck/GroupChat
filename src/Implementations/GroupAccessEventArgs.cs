using GroupChat.Shared.Domain;

namespace GroupChat.Implementations
{
    public class GroupAccessEventArgs : System.EventArgs
    {
        public GroupAccessRequest AccessRequest { get; init; }

        public GroupAccessEventArgs(GroupAccessRequest accessRequest) => AccessRequest = accessRequest;
    }
}