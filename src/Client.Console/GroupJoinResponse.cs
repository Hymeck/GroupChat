using System.Net;
using Microsoft.FSharp.Core;

namespace GroupChat.Client.Console
{
    public class GroupJoinResponse
    {
        public ResponseCode Code { get; init; }
        public FSharpOption<IPEndPoint> GroupEndpoint { get; init; }

        public GroupJoinResponse()
        {
        }

        public GroupJoinResponse(ResponseCode code, FSharpOption<IPEndPoint> groupEndpoint)
        {
            Code = code;
            GroupEndpoint = groupEndpoint;
        }
    }
}