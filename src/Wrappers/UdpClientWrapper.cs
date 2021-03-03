using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Configures <see cref="UdpClient"/> to send and receive datagrams.
    /// </summary>
    public abstract class UdpClientWrapper
    {
        #region fields
        
        /// <summary>
        /// Holds multicast endpoint which contains multicast IP address and port.
        /// <remarks>Also it may be configured as multicast, broadcast, etc. </remarks>
        /// </summary>
        protected readonly IPEndPoint _remoteEndpoint;

        /// <summary>
        /// Used to send and receive UDP datagrams.
        /// </summary>
        public readonly UdpClient Client;
        
        /// <summary>
        /// Indicates whether listening of receiving data has begun or not.
        /// </summary>
        protected bool _beginReceived;
        
        #endregion fields

        #region constructor

        /// <summary>
        /// Initializes a new instance of <see cref="UdpClientWrapper"/> class with specified <paramref name="remoteIpAddress"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// <remarks>If you don't know your real local IP address pass <paramref name="localIpAddress"/> as null.</remarks> 
        /// </summary>
        /// <param name="remoteIpAddress">The multicast IP address.</param>
        /// <param name="port">The port.</param>
        /// <param name="localIpAddress">The local IP address.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="remoteIpAddress"/> is null.</exception>
        protected UdpClientWrapper(IPAddress remoteIpAddress, int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (remoteIpAddress == null)
                throw new ArgumentNullException(nameof(remoteIpAddress));

            var localIp = localIpAddress ?? IPAddress.Any;
            _remoteEndpoint = new IPEndPoint(remoteIpAddress, port);

            Client = new UdpClient();
            // allow multiple clients in the same PC
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            var localEndpoint = new IPEndPoint(localIp, port);
            Client.Client.Bind(localEndpoint);
        }

        #endregion constructor
        
        #region public methods

        /// <summary>
        /// Setups instance to receive data.
        /// <remarks>Call it once.</remarks>
        /// </summary>
        public virtual void BeginReceive()
        {
            if (_beginReceived)
                return;

            // begin listen for received data
            _beginReceived = true;
            CallUdpClientBeginReceive();
        }
        
        /// Calls <see cref="Send(byte[], IPEndPoint)"/> with <see cref="IPEndPoint"/> specified by remote IP address.
        /// <remarks>Use it for broadcast or multicast data sends.</remarks>
        public virtual void Send(byte[] data)
        {
            Send(data, _remoteEndpoint);
        }

        /// <summary>
        /// Sends data to destination endpoint.
        /// </summary>
        /// <param name="data">/The data to be sent.</param>
        /// <param name="remoteEndpoint">Destination for sent data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        public virtual void Send(byte[] data, IPEndPoint remoteEndpoint)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Sent data is null or empty.");

            CallUdpClientBeginSend(data, remoteEndpoint);
        }
        
        /// <summary>
        /// Closes udp client.
        /// </summary>
        public virtual void Close()
        {
            Client.Close();
        }
        
        #endregion public methods

        #region private methods

        /// <summary>
        /// Invokes datagram receiving.
        /// </summary>
        private void CallUdpClientBeginReceive() => Client.BeginReceive(ReceivedCallback, state: null);

        /// <summary>
        /// Invokes when datagram received.
        /// </summary>
        /// <param name="result">An object returned by a call of _client.BeginReceive.</param>
        private void ReceivedCallback(IAsyncResult result)
        {
            var senderEndpoint = new IPEndPoint(0, 0);
            var receivedData = Client.EndReceive(result, ref senderEndpoint);

            // trigger event if subscribed 
            DatagramReceived?.Invoke(this, new DatagramReceivedEventArgs(receivedData, senderEndpoint));
            // restart listening for UDP incomes
            CallUdpClientBeginReceive();
        }


        /// <summary>
        /// Invokes when datagram sent.
        /// </summary>
        /// <param name="result">An object returned by a call of _client.BeginSend.</param>
        private void SentCallback(IAsyncResult result)
        {
            Client.EndSend(result);
        }

        /// <summary>
        /// Invokes datagram sending.
        /// </summary>
        /// <param name="datagram">Datagram to be sent.</param>
        /// <param name="remoteEp">Destination for sent datagram.</param>
        private void CallUdpClientBeginSend(byte[] datagram, IPEndPoint remoteEp) =>
            Client.BeginSend(datagram, datagram.Length, remoteEp, SentCallback, state: null);
        
        #endregion private methods
        
        /// <summary>
        /// Event occuring when datagram received.
        /// </summary>
        public event EventHandler<DatagramReceivedEventArgs> DatagramReceived;
    }
}