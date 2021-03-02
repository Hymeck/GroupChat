#nullable enable
using System;
using System.Net;
using GroupChat.Extensions;
using GroupChat.Implementations.Dtos;
using GroupChat.Shared.Domain;
using GroupChat.Shared.Interfaces;
using GroupChat.Shared.Wrappers;
using Microsoft.FSharp.Core;

namespace GroupChat.Implementations
{
    public partial class GroupParticipant
    {
        private readonly BroadcastUdpClient _networkClient;
        private MulticastUdpClient _chatClient = null;
        private int _multicastPort;
        private IPAddress _localIpAddress;

        public GroupParticipant(int broadcastPort, int multicastPort, IPAddress localIpAddress = null)
        {
            _localIpAddress = localIpAddress;
            _networkClient = new BroadcastUdpClient(broadcastPort, _localIpAddress);
            _multicastPort = multicastPort;
        }
    }

    #region creator impementation

    public partial class GroupParticipant : IGroupCreator<GroupAccessEventArgs>
    {
        private string _groupId = null;
        private IPAddress _multicastIpAddress = null;
        
        public IPEndPoint? CreateGroup(string groupId)
        {
            // todo: check existence
            _groupId = groupId;
            
            // todo: check freedom of multicast ip
            var multicastIpAddress = IPAddress.Parse("224.0.0.0");
            _multicastIpAddress = multicastIpAddress;

            _chatClient = new MulticastUdpClient(_multicastIpAddress, _multicastPort, _localIpAddress);
            _chatClient.BeginReceive();
            
            _chatClient.DatagramReceived += (sender, args) =>
            {
                // todo: access or deny
                var accessResponse = new GroupAccessResponse(Result.Yes,
                    FSharpOption<IPEndPoint>.Some(new IPEndPoint(_multicastIpAddress, _multicastPort)));
                
                // todo: bleat, this is unicast, dude. correct crap.
                _networkClient.Send(accessResponse.XmlSerialize());
            };
            
            throw new NotImplementedException();
        }

        public bool DestroyGroup()
        {
            // todo: notify members
            // todo: set null to multicast wrapper
            throw new NotImplementedException();
        }

        public event EventHandler<GroupAccessEventArgs> GroupAccessRequestReceived;
    }

    #endregion creator impementation
    
    #region participant implementation

    // todo: client to message exchanging in group should be null before joining? 
    public partial class GroupParticipant : IParticipant<Message, MessageEventArgs<Message>>
    {
        public void SendMessage(Message message)
        {
            // todo: parse message to bytes
            // todo: send message
            throw new NotImplementedException();
        }

        public bool LeaveGroup()
        {
            // todo: if creator call DestroyGroup()
            // todo: if simple participant notify participants about exiting
            // set null to multicast wrapper
            throw new NotImplementedException();
        }

        public event EventHandler<MessageEventArgs<Message>> MessageReceived;
    }

    #endregion
}