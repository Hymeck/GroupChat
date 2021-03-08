using System.Net;

namespace GroupChat.Client.Console
{
    public class JoinRequestElement
    {
        public GroupJoinRequest Request { get; init; }
        public IPEndPoint From { get; init; }

        public JoinRequestElement()
        {
        }

        public JoinRequestElement(GroupJoinRequest request, IPEndPoint from)
        {
            Request = request;
            From = from;
        }
    }
}