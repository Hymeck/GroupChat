using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    public class PeerceClient
    {
        public readonly string Username;
        private BroadcastUdpClient _broadcast;
        private MulticastUdpClient _multicast;

        public PeerceClient(string username, int port = 9000, IPAddress localIpAddress = null)
        {
            Username = username;
            
            ConfigureBroadcast(port, localIpAddress);
        }

        private void ConfigureBroadcast(int port, IPAddress localIpAddress)
        {
            if (_broadcast != null)
            {
                _broadcast.Close();
                _broadcast = null;
            }

            _broadcast = new BroadcastUdpClient(port, localIpAddress);
            _broadcast.DatagramReceived += OnBroadcastDatagramReceived;
            // _broadcast.UdpClient.Client.ReceiveTimeout = 10000; // set timeout of sync receive (after 10 s will be raised SocketException)
            _broadcast.BeginReceive();
        }

        private IPEndPoint _cachedEndpoint;
        private void OnBroadcastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var gjr = e.Datagram.XmlDeserialize<GroupJoinRequest>();

            // ignore request if we are not group creator and specified group id don't equal to this._groupId 
            if (!_isGroupCreator || gjr.GroupId != _groupId)
                return;

            var requestArgs = new GroupJoinRequestEventArgs(gjr.Username, gjr.GroupId, gjr.SentAt, e.From.Address);
            _cachedEndpoint = e.From;
            
            GroupJoinRequestReceived?.Invoke(this, requestArgs);
            // var requestTask = Task.Run(() => GroupJoinRequestReceived?.Invoke(this, requestArgs));
        }

        public void Accept()
        {
            if (_cachedEndpoint == null)
                return;
            
            lock (_cachedEndpoint)
            {
                var multicastEp = _multicast.DestinationEndpoint;
                var joinResponse = new GroupJoinResponse(ResponseCode.Success, multicastEp);
                _broadcast.Send(joinResponse.XmlSerialize(), _cachedEndpoint);
                _cachedEndpoint = null;
            }
        }

        private void ConfigureMulticast(IPAddress multicastIpAddress, int port, IPAddress localIpAddress)
        {
            if (_multicast != null)
            {
                _multicast.Close();
                _multicast = null;
            }

            _multicast = new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicast.DatagramReceived += OnMulticastDatagramReceived;
            _multicast.BeginReceive();
        }

        private void OnMulticastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var msg = e.Datagram.XmlDeserialize<GroupMessage>();
            
            var msgArgs =
                new GroupMessageEventArgs(msg.Username, _groupId, msg.Text, msg.SentAt, e.From.Address);
            
            GroupMessageReceived?.Invoke(this, msgArgs);
        }
        
        private string _groupId;
        private bool _isGroupCreator;
        
        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100)
        {
            _groupId = groupId;
            _isGroupCreator = true;
            
            ConfigureMulticast(multicastIpAddress, port, GetLocalIpAddress());
        }

        public void JoinGroup(string groupId)
        {
            var joinRequest = new GroupJoinRequest(Username, groupId, DateTime.Now).XmlSerialize();
            _broadcast.Send(joinRequest);
            
            // todo: remake it: blocks UI
            try
            {
                IPEndPoint creatorEp = null;
                var joinResponse = _broadcast.UdpClient.Receive(ref creatorEp);
                
                var result = joinResponse.XmlDeserialize<GroupJoinResponse>();

                if (!result.Code.IsSuccess())
                    throw new GroupJoinDeniedException();
                
                _groupId = groupId;
                var groupEp = result.GroupEndpoint.Value;
                
                ConfigureMulticast(groupEp.Address, groupEp.Port, GetLocalIpAddress());
            }

            catch (SocketException)
            {
                // time is out, no response
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
        
        public void Finish()
        {
            _broadcast?.Close();
            _multicast?.Close();
        }

        public event EventHandler<GroupJoinRequestEventArgs> GroupJoinRequestReceived;
        public event EventHandler<GroupMessageEventArgs> GroupMessageReceived;
        
        // public delegate bool 
    }
}