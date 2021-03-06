using System.Net;
using Microsoft.FSharp.Core;

namespace GroupChat.Client.Console
{
    public class GroupJoinResponse
    {
        public ResponseCode Code { get; init; }
        public string GroupId { get; init; }
        public FSharpOption<IPEndPoint> GroupEndpoint { get; init; }

        public GroupJoinResponse()
        {
        }

        public GroupJoinResponse(ResponseCode code, string groupId, FSharpOption<IPEndPoint> groupEndpoint)
        {
            Code = code;
            GroupId = groupId;
            GroupEndpoint = groupEndpoint;
        }
    }
}