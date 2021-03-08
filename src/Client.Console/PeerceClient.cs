using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;
using Microsoft.FSharp.Core;

namespace GroupChat.Client.Console
{
    public partial class PeerceClient
    {
        public readonly string Username;

        private string _groupId;
        private bool _isGroupCreator;
        private bool _isGroupParticipant;
        
        private ConcurrentQueue<(GroupJoinRequest, IPEndPoint)> _joinRequestQueue;

        private BroadcastUdpClientWrapper _broadcast;
        private MulticastUdpClient _multicast;

        public bool IsGroupParticipant => _isGroupParticipant;

        public PeerceClient(string username, int port = 9000, IPAddress localIpAddress = null)
        {
            Username = username;
            
            InitBroadcast(port, localIpAddress);
        }

        private void InitBroadcast(int port, IPAddress localIpAddress)
        {
            _broadcast?.UdpClient.Dispose();

            _broadcast = new BroadcastUdpClientWrapper(port, localIpAddress);
        }

        public void StartBroadcastReceiving(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var receiveResult = await _broadcast.UdpClient.ReceiveAsync();
                    ProcessReceiveResult(receiveResult.Buffer, receiveResult.RemoteEndPoint);
                }
            }, cancellationToken);
        }

        private void ProcessReceiveResult(byte[] datagram, IPEndPoint remoteEndpoint)
        {
            // as creator, i am waiting for group join requests
            if (TryDeserializeDatagram(datagram, out GroupJoinRequest joinRequest)) 
                HandleJoinRequest(joinRequest, remoteEndpoint);
        }
        
        private static bool TryDeserializeDatagram<T>(byte[] datagram, out T result) where T : class
        {
            try
            {
                result = datagram.XmlDeserialize<T>();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private void InitMulticast(IPAddress multicastIpAddress, int port, IPAddress localIpAddress)
        {
            _multicast?.Dispose();
            _multicast = null;

            _multicast ??= new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicast.DatagramReceived += OnMulticastDatagramReceived;
            _multicast.BeginReceive();
        }

        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100)
        {
            _groupId = groupId;
            _isGroupCreator = _isGroupParticipant = true;

            InitMulticast(multicastIpAddress, port, GetLocalIpAddress());
            
            _joinRequestQueue?.Clear();
            _joinRequestQueue ??= new ConcurrentQueue<(GroupJoinRequest, IPEndPoint)>();
        }

        private IPAddress GetLocalIpAddress() => _broadcast.LocalEndpoint.Address;
        
        public void Close()
        {
            _broadcast?.UdpClient.Dispose();
            _multicast?.Dispose();
        }
    }

    #region broadcast handling

    public partial class PeerceClient
    {
        public async Task JoinGroup(string groupId, CancellationToken cancellationToken)
        {
            var datagram = new GroupJoinRequest(Username, groupId, DateTime.Now).XmlSerialize();
            
            await _broadcast.UdpClient.SendAsync(datagram, datagram.Length, _broadcast.DestinationEndpoint).ConfigureAwait(false);
            var receiveResult = await _broadcast.UdpClient.ReceiveAsync(60000, cancellationToken).ConfigureAwait(false);

            var response = receiveResult.Buffer.XmlDeserialize<GroupJoinResponse>();
            
            HandleJoinResponse(response);
        }

        public async Task CheckGroupJoinRequests(Func<GroupJoinRequest, IPEndPoint, bool> choice)
        {
            // ignore if i am not a creator
            if (!_isGroupCreator)
                return;
            
            //todo: remove hardcoded console IO
            while (_joinRequestQueue.TryDequeue(out var item))
            {
                var groupJoinRequest = item.Item1;
                var fromEndpoint = item.Item2;

                var isAccept = choice(groupJoinRequest, fromEndpoint);
                var responseCode = isAccept.ToResponseCode(); 
                
                var responseDatagram =
                    new GroupJoinResponse(
                            responseCode, 
                            _groupId, 
                            _multicast.DestinationEndpoint.ToString())
                        .XmlSerialize();

                // await _broadcast.UdpClient.SendAsync(responseDatagram, responseDatagram.Length, from);
                await _broadcast.UdpClient.SendAsync(responseDatagram, responseDatagram.Length, _broadcast.DestinationEndpoint);

                if (isAccept)
                {
                    var joinedMessage = new GroupMessage(groupJoinRequest.Username, $"{groupJoinRequest.Username} joined!",
                        DateTime.Now);
                    _multicast.Send(joinedMessage.XmlSerialize());
                }
            }
        }
        
        private void HandleJoinResponse(GroupJoinResponse joinResponse)
        {
            var code = joinResponse.Code;
            
            if (code.IsSuccess())
                DoJoinGroup(IPEndPoint.Parse(joinResponse.GroupIpEndpoint), joinResponse.GroupId);

            else if (code.IsFail())
                throw new GroupJoinDeniedException();
        }

        private void DoJoinGroup(IPEndPoint groupEndpoint, string groupId)
        {
            _groupId = groupId;
            _isGroupParticipant = true;
            
            InitMulticast(groupEndpoint.Address, groupEndpoint.Port, GetLocalIpAddress());
        }

        private void HandleJoinRequest(GroupJoinRequest joinRequest, IPEndPoint from)
        {
            // ignore me-to-me message
            if (joinRequest.Username == Username)
                return;

            // ignore if requested group id does not match with mine
            if (joinRequest.GroupId != _groupId)
                return;
            
            // ignore if i am not a creator
            if (!_isGroupCreator)
                return;
            
            // so i am creator of request group. add to the queue
            _joinRequestQueue.Enqueue((joinRequest, from));
        }
    }

    #endregion broadcast handling

    #region group messaging

    public partial class PeerceClient
    {
        public void SendMessage(string text)
        {
            if (_multicast == null)
                return;

            var msg = new GroupMessage(Username, text, DateTime.Now);
            _multicast.Send(msg.XmlSerialize());
        }
        
        private void OnMulticastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var msg = e.Datagram.XmlDeserialize<GroupMessage>();
            
            var msgArgs =
                new GroupMessageEventArgs(msg.Username, _groupId, msg.Text, msg.SentAt, e.From.Address);
            
            GroupMessageReceived?.Invoke(this, msgArgs);
        }
        
        public event EventHandler<GroupMessageEventArgs> GroupMessageReceived;
    }

    #endregion group messaging
}