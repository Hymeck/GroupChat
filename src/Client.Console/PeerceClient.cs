using System;
using System.Collections.Concurrent;
using System.Net;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    public class PeerceClient
    {
        public readonly string Username;

        private string _groupId;
        private bool _isGroupCreator;
        
        private ConcurrentQueue<JoinQueueElement> _joinRequestQueue;

        private BroadcastUdpClientWrapper _broadcast;
        private MulticastUdpClient _multicast;
        
        public bool IsGroupParticipant => _multicast != null;

        public PeerceClient(string username, int port = 9000, IPAddress localIpAddress = null)
        {
            Username = username;
            
            ConfigureBroadcast(port, localIpAddress);
        }

        private void ConfigureBroadcast(int port, IPAddress localIpAddress)
        {
            _broadcast?.UdpClient.Dispose();

            _broadcast = new BroadcastUdpClientWrapper(port, localIpAddress);
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
        
        private void AddJoinRequestToQueue(GroupJoinRequest groupJoinRequest, IPEndPoint from)
        {
            if (_multicast == null)
                return;
            
            // ignore request if we are not group creator and specified group id don't equal to this._groupId 
            if (!_isGroupCreator || groupJoinRequest.GroupId != _groupId)
                return;
            
            _joinRequestQueue.Enqueue(new JoinQueueElement(groupJoinRequest, from));
        }

        private void SetGroup(GroupJoinResponse groupJoinResponse)
        {
            // ignore if not success code
            if (!groupJoinResponse.Code.IsSuccess())
                return;

            // ignore if already in this group
            if (_groupId == groupJoinResponse.GroupId)
                return;
            
            _groupId = groupJoinResponse.GroupId;
            var groupEp = groupJoinResponse.GroupEndpoint.Value;
                
            ConfigureMulticast(groupEp.Address, groupEp.Port, GetLocalIpAddress());
        }
        
        // private void OnBroadcastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        // {
        //     var datagram = e.Datagram;
        //     if (TryDeserializeDatagram(datagram, out GroupJoinRequest joinRequest))
        //     {
        //         AddJoinRequestToQueue(joinRequest, e.From);
        //     }
        //     
        //     else if (TryDeserializeDatagram(datagram, out GroupJoinResponse joinResponse))
        //     {
        //         SetGroup(joinResponse);
        //     }
        // }

        public void HandleJoinRequestQueue()
        {
            if (_multicast == null)
                return;
            
            while (_joinRequestQueue.TryDequeue(out var el))
            {
                var request = new GroupJoinRequestEventArgs(
                    el.Request.Username, 
                    el.Request.GroupId, 
                    el.Request.SentAt,
                    el.From);
                
                GroupJoinRequestReceived?.Invoke(this, request);
            }
        }
        
        public void Accept(GroupJoinRequestEventArgs e)
        {
            if (_multicast == null)
                return;

            if (!_isGroupCreator || e.GroupId != _groupId)
                return;
            
            var multicastEp = _multicast.DestinationEndpoint;
            var joinResponse = new GroupJoinResponse(ResponseCode.Success, _groupId, multicastEp);
            // _broadcast.Send(joinResponse.XmlSerialize(), e.From);
        }

        private void ConfigureMulticast(IPAddress multicastIpAddress, int port, IPAddress localIpAddress)
        {
            _multicast?.Dispose();
            _joinRequestQueue?.Clear();

            _multicast = new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicast.DatagramReceived += OnMulticastDatagramReceived;
            _multicast.BeginReceive();

            _joinRequestQueue = new ConcurrentQueue<JoinQueueElement>();
        }
        
        private void OnMulticastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var msg = e.Datagram.XmlDeserialize<GroupMessage>();
            
            var msgArgs =
                new GroupMessageEventArgs(msg.Username, _groupId, msg.Text, msg.SentAt, e.From.Address);
            
            GroupMessageReceived?.Invoke(this, msgArgs);
        }

        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100)
        {
            _groupId = groupId;
            _isGroupCreator = true;
            
            ConfigureMulticast(multicastIpAddress, port, GetLocalIpAddress());
        }

        public void JoinGroup(string groupId, int timeout = 10000)
        {
            var joinRequest = new GroupJoinRequest(Username, groupId, DateTime.Now).XmlSerialize();
            // _broadcast.Send(joinRequest);
            _broadcast.UdpClient.Send(joinRequest, joinRequest.Length, _broadcast.DestinationEndpoint);

            var previousTimeout = _broadcast.UdpClient.Client.ReceiveTimeout;
            try
            {
                IPEndPoint creatorEp = null;
                // _broadcast.UdpClient.Client.ReceiveTimeout = timeout;
                
                var result = _broadcast.UdpClient.Receive(ref creatorEp);
                var response = result.XmlDeserialize<GroupJoinResponse>();
                
                SetGroup(response);
            }

            finally
            {
                _broadcast.UdpClient.Client.ReceiveTimeout = previousTimeout;
            }
        }

        private IPAddress GetLocalIpAddress()
        {
            var localIpEp = (IPEndPoint)_broadcast.UdpClient.Client.LocalEndPoint;
            return localIpEp.Address;
        }
        
        public void SendMessage(string text)
        {
            if (_multicast == null)
                return;

            var msg = new GroupMessage(Username, text, DateTime.Now);
            _multicast.Send(msg.XmlSerialize());
        }
        
        public void Close()
        {
            _broadcast?.UdpClient.Dispose();
            _multicast?.Dispose();
        }

        public event EventHandler<GroupJoinRequestEventArgs> GroupJoinRequestReceived;
        public event EventHandler<GroupMessageEventArgs> GroupMessageReceived;
    }
}