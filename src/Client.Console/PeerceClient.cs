using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    public partial class PeerceClient
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

            _multicast = new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicast.DatagramReceived += OnMulticastDatagramReceived;
            _multicast.BeginReceive();

        }

        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100)
        {
            _groupId = groupId;
            _isGroupCreator = true;
            
            InitMulticast(multicastIpAddress, port, GetLocalIpAddress());
        }

        private IPAddress GetLocalIpAddress() => _broadcast.LocalEndpoint.Address;
        
        
        public void Close()
        {
            _broadcast?.UdpClient.Dispose();
            _multicast?.Dispose();
        }
    }

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

    #region broadcast handling

    public partial class PeerceClient
    {
        // public event EventHandler<GroupJoinRequestEventArgs> GroupJoinRequestReceived;
    }

    #endregion broadcast handling

}