using System;
using System.Linq;
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
        public readonly string Username;

        public GroupParticipant(string username, int broadcastPort, int multicastPort, IPAddress localIpAddress = null)
        {
            Username = username;

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

        public bool CreateGroup(string groupId)
        {
            // todo: 1. check existence
            
            _groupId = groupId;

            // todo: 2. check freedom of multicast ip
            
            var multicastIpAddress = IPAddress.Parse("224.0.0.0");
            _multicastIpAddress = multicastIpAddress;

            _chatClient = new MulticastUdpClient(_multicastIpAddress, _multicastPort, _localIpAddress);
            _chatClient.DatagramReceived += OnGroupDatagramReceived;
            _chatClient.BeginReceive();


            return true;
        }

        public bool DestroyGroup()
        {
            // todo: notify members
            
            _chatClient.Close();
            _chatClient = null;
            
            return true;
        }

        private void OnGroupDatagramReceived(object sender, DatagramReceivedEventArgs args)
        {
            var message = args.Datagram.XmlDeserialize<Message>();
            
            MessageReceived?.Invoke(this, new MessageEventArgs<Message>(message));
            // Console.WriteLine(message);
            
            // var message = args.Datagram;
            // var remoteEp = args.From;
            //
            // // todo: access or deny
            //
            // var accessResponse = new GroupAccessResponse(Result.Yes,
            //     FSharpOption<IPEndPoint>.Some(new IPEndPoint(_multicastIpAddress, _multicastPort)));
            //
            // var messageCode = GetMessageCode<GroupAccessResponse>();
            //
            // var responseDatagram = PrependMessageCode(accessResponse.XmlSerialize(), messageCode);
            //
            // _networkClient.Send(responseDatagram, remoteEp);
        }

        private static byte[] PrependMessageCode(byte[] source, byte messageCode) =>
            source
                .Prepend(messageCode)
                .ToArray();


        private static byte GetMessageCode<TMessage>() =>
            (byte) Mapper.AssemblyTypes
                .Where(t => t == typeof(TMessage))
                .Select((t, i) => i)
                .FirstOrDefault();

        public event EventHandler<GroupAccessEventArgs> GroupAccessRequestReceived;
    }

    #endregion creator impementation

    #region participant implementation

    // todo: client to message exchanging in group should be null before joining? 
    public partial class GroupParticipant : IParticipant<Message, MessageEventArgs<Message>>
    {
        public void SendMessage(Message message)
        {
            if (_chatClient == null)
                return;

            var messageDatagram = message.XmlSerialize();
            
            _chatClient.Send(messageDatagram);
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