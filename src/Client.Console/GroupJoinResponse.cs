#nullable enable
using System.Net;
using Microsoft.FSharp.Core;

namespace GroupChat.Client.Console
{
    public class GroupJoinResponse
    {
        public ResponseCode Code { get; init; }
        public string GroupId { get; init; }
        public string? GroupIpEndpoint { get; init; }
        
#pragma warning disable 8618
        public GroupJoinResponse()
#pragma warning restore 8618
        {
        }

        public GroupJoinResponse(ResponseCode code, string groupId, string? groupIpEndpoint)
        {
            Code = code;
            GroupId = groupId;
            GroupIpEndpoint = groupIpEndpoint;
        }
    }
}