namespace GroupChat.Shared.Domain

open System
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

module Mapper =
    
    /// Gets types of GroupChat.Shared.Domain namespace.
    let AssemblyTypes: Type[] =
        (typeof<GroupExistRequest>.Assembly).GetTypes()
    
    /// Maps types to their string representations.
    let private typesToStrings (types: Type[]): seq<string> =
        types |> Seq.map (fun t -> t.ToString()) 
    
    let private byteSeq (length: int): seq<byte> =
        seq { for value in 1 .. length -> byte value }
    
    /// Makes table with byte key and Type value. (Need for deserialization)
    let ByteTypeTable: Map<byte, Type> =
        let types = AssemblyTypes
        let byteValues = byteSeq types.Length
        Map(types |> Seq.zip byteValues)
    
    /// Need for 
    let StringTypeTable: Map<string, Type> =
        let types = AssemblyTypes
        let strTypes = types |> typesToStrings
        Map(types |> Seq.zip strTypes)
    
    /// Makes table with string key and byte value. (Need for serialization)
    let StringByteTable: Map<string, byte> =
        let types = AssemblyTypes
        let strTypes = types |> typesToStrings
        let byteValues = byteSeq types.Length
        Map(byteValues |> Seq.zip strTypes)
    
//    let toByte (t: Type): byte =
//        let table = ByteTypeTable
//        for 
//        byte 0