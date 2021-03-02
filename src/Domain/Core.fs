namespace GroupChat.Shared.Domain

open System.Net

type GroupExistRequest = { ChatId: string }

type IpAddressReservedRequest = { MulticastIpAddress: IPAddress }

type GroupAccessRequest = { Username: string; ChatId: string }

type Result =
    | Undefined
    | No
    | Yes

type GroupExistResponse = { Exist: Result }

type IpAddressReservedResponse = { Reserved: Result }

type GroupAccessResponse =
    { Access: Result
      ChatEndpoint: Option<IPEndPoint> }
