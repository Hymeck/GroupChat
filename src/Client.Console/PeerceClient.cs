using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    /// <summary>
    /// Provides methods for group messaging via UDP
    /// </summary>
    public partial class PeerceClient
    {
        /// <summary>
        /// Holds specified username in constructor.
        /// </summary>
        public readonly string Username;
        
        /// <summary>
        /// Holds group id.
        /// <remarks>It is should be null if there is no either group creator nor just group participant.</remarks>
        /// </summary>
        private string _groupId;
        
        /// <summary>
        /// Flag for indicating of group creator.
        /// <remarks>It is should be false when there is no group creator.</remarks>
        /// </summary>
        private bool _isGroupCreator;
        
        /// <summary>
        /// Flag for indicating group membership.
        /// <remarks>It is should be false when there is no group membership.</remarks>
        /// </summary>
        private bool _isGroupParticipant;
        
        /// <summary>
        /// Holds requests to access a group. Used in <see cref="HandleJoinRequest"/> method.
        /// <remarks>It is instantiated when group created. Otherwise, it is should be null.</remarks>
        /// </summary>
        private ConcurrentQueue<(GroupJoinRequest, IPEndPoint)> _joinRequestQueue;

        /// <summary>
        /// Service used for sending broadcast messages.
        /// </summary>
        private BroadcastUdpClientWrapper _broadcast;
        
        /// <summary>
        /// Service used for message exchanging in a group.
        /// <remarks>It is instantiated when group created or group joined.</remarks>
        /// </summary>
        private MulticastUdpClient _multicast;

        /// <summary>
        /// Gets a value indicating whether there is group membership.
        /// </summary>
        /// <returns>True if there is group membership; otherwise, false.</returns>
        public bool IsGroupParticipant => _isGroupParticipant;

        /// <summary>
        /// Initialized a new instance with specified <paramref name="username"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// Configures underlying broadcast service. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="port"></param>
        /// <param name="localIpAddress"></param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is null or an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        public PeerceClient(string username, int port = 9000, IPAddress localIpAddress = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException($"{username} is null or empty.", nameof(username));
            Username = username;
            
            InitBroadcast(port, localIpAddress);
        }

        /// <summary>
        /// Releases resources from previous <see cref="System.Net.Sockets.UdpClient"/> object. Initializes <see cref="_broadcast"/>.
        /// </summary>
        /// <param name="port">A port used for broadcast messaging.</param>
        /// <param name="localIpAddress">The local IP address.</param>
        private void InitBroadcast(int port, IPAddress localIpAddress)
        {
            _broadcast?.UdpClient.Dispose();

            _broadcast = new BroadcastUdpClientWrapper(port, localIpAddress);
        }

        /// <summary>
        /// Runs underlying broadcast service to receive broadcast messages asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
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

        /// <summary>
        /// Interprets received datagram and calls corresponding methods for handling achieved result.
        /// </summary>
        /// <param name="datagram">A datagram received in <see cref="StartBroadcastReceiving"/> loop.</param>
        /// <param name="remoteEndpoint">An <see cref="IPEndPoint"/> object from which <paramref name="datagram"/> was sent.</param>
        private void ProcessReceiveResult(byte[] datagram, IPEndPoint remoteEndpoint)
        {
            // as creator, i am waiting for group join requests
            if (TryDeserializeDatagram(datagram, out GroupJoinRequest joinRequest)) 
                HandleJoinRequest(joinRequest, remoteEndpoint);
        }
        
        /// <summary>
        /// Tries to deserialize specified <paramref name="datagram"/>.
        /// </summary>
        /// <param name="datagram">A datagram to be deserialized.</param>
        /// <param name="result">An object obtained by deserialization.</param>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <returns>Deserialized object if it was successful; otherwise, null.</returns>
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

        /// <summary>
        /// Releases resources from previous <see cref="MulticastUdpClient"/> object.
        /// Initializes <see cref="_multicast"/>.
        /// Subscribes to message received event.
        /// Starts to receiving incoming group messages.
        /// </summary>
        /// <param name="multicastIpAddress">A multicast IP address used for group.</param>
        /// <param name="port">A port used for group.</param>
        /// <param name="localIpAddress">The local IP address.</param>
        private void InitMulticast(IPAddress multicastIpAddress, int port, IPAddress localIpAddress)
        {
            _multicast?.Dispose();

            _multicast = new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicast.DatagramReceived += OnMulticastDatagramReceived;
            _multicast.BeginReceive();
        }

        /// <summary>
        /// Creates group with specified <paramref name="groupId"/>, <paramref name="multicastIpAddress"/> and <paramref name="port"/>.
        /// Setups underlying multicast service.
        /// Initializes queue for group join requests.
        /// </summary>
        /// <param name="groupId">A group id to be used for message exchanging.</param>
        /// <param name="multicastIpAddress">A multicast IP address to be used for group.</param>
        /// <param name="port">A port used for group.</param>
        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100)
        {
            _groupId = groupId;
            _isGroupCreator = _isGroupParticipant = true;

            InitMulticast(multicastIpAddress, port, GetLocalIpAddress());
            
            _joinRequestQueue?.Clear();
            _joinRequestQueue ??= new ConcurrentQueue<(GroupJoinRequest, IPEndPoint)>();
        }

        /// <summary>
        /// Gets the local IP address.
        /// </summary>
        /// <returns><see cref="IPAddress"/> object corresponding to local IP address.</returns>
        private IPAddress GetLocalIpAddress() => _broadcast.LocalEndpoint.Address;

        /// <summary>
        /// Releases resources of underlying services.
        /// </summary>
        public void Close()
        {
            _broadcast?.UdpClient.Dispose();
            _multicast?.Dispose();
        }
    }

    #region broadcast handling

    public partial class PeerceClient
    {
        /// <summary>
        /// Sends request to join to the group with specified <paramref name="groupId"/>.
        /// If successful, gets creator response and behaves depending on it's <see cref="ResponseCode"/> code.
        /// <remarks>By default, waiting time is 60 seconds. After time's out will be thrown <see cref="TaskCanceledException"/> exception.</remarks>
        /// </summary>
        /// <param name="groupId">A group id to join.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task JoinGroup(string groupId, CancellationToken cancellationToken)
        {
            var datagram = new GroupJoinRequest(Username, groupId, DateTime.Now).XmlSerialize();
            
            await _broadcast.UdpClient.SendAsync(datagram, datagram.Length, _broadcast.DestinationEndpoint).ConfigureAwait(false);
            var receiveResult = await _broadcast.UdpClient.ReceiveAsync(60000, cancellationToken).ConfigureAwait(false);

            var response = receiveResult.Buffer.XmlDeserialize<GroupJoinResponse>();
            
            HandleJoinResponse(response);
        }

        /// <summary>
        /// Processes group join request queue if it is not empty.
        /// Sends message if requester was joined to a group.
        /// </summary>
        /// <param name="choice">A function returning creator's decision about accepting/rejecting requesters.</param>
        /// <remarks>Works if there is group creator.</remarks>
        public async Task ProcessGroupJoinRequests(Func<GroupJoinRequest, IPEndPoint, bool> choice)
        {
            // ignore if i am not a creator
            if (!_isGroupCreator)
                return;
            
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
                throw new GroupJoinRejectedException();
        }

        private void DoJoinGroup(IPEndPoint groupEndpoint, string groupId)
        {
            _groupId = groupId;
            _isGroupParticipant = true;
            
            InitMulticast(groupEndpoint.Address, groupEndpoint.Port, GetLocalIpAddress());
        }

        /// <summary>
        /// Checks and it possible adds <paramref name="joinRequest"/> to group join request queue.
        /// </summary>
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
        /// <summary>
        /// Sends message to other group participants
        /// <remarks>Works if there is group membership.</remarks>
        /// </summary>
        /// <param name="text">A message text to be sent.</param>
        public void SendMessage(string text)
        {
            if (!_isGroupParticipant)
                return;

            var msg = new GroupMessage(Username, text, DateTime.Now);
            _multicast.Send(msg.XmlSerialize());
        }
        
        /// <summary>
        /// Fires <see cref="GroupMessageReceived"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMulticastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var msg = e.Datagram.XmlDeserialize<GroupMessage>();
            
            var msgArgs =
                new GroupMessageEventArgs(msg.Username, _groupId, msg.Text, msg.SentAt, e.From.Address);
            
            GroupMessageReceived?.Invoke(this, msgArgs);
        }
        
        /// <summary>
        /// Occurs when group message received.
        /// </summary>
        public event EventHandler<GroupMessageEventArgs> GroupMessageReceived;
    }

    #endregion group messaging
}