using System.Net;

namespace GroupChat.Client.Console
{
    public class JoinQueueElement
    {
        public GroupJoinRequest Request { get; init; }
        public IPEndPoint From { get; init; }

        public JoinQueueElement()
        {
        }

        public JoinQueueElement(GroupJoinRequest request, IPEndPoint from)
        {
            Request = request;
            From = from;
        }
    }
}