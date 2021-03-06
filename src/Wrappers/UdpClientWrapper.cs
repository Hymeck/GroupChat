using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Configures <see cref="System.Net.Sockets.UdpClient"/> to send and receive datagrams.
    /// </summary>
    public abstract class UdpClientWrapper
    {
        #region fields
        
        /// <summary>
        /// Holds destination endpoint.
        /// </summary>
        protected readonly IPEndPoint _destinationEndpoint;

        /// <summary>
        /// Used to send and receive UDP datagrams.
        /// </summary>
        public readonly UdpClient UdpClient;
        
        /// <summary>
        /// Indicates whether listening of receiving data has begun or not.
        /// </summary>
        protected bool _beginReceived;
        
        #endregion fields

        public IPEndPoint DestinationEndpoint => _destinationEndpoint;

        #region constructor

        /// <summary>
        /// Initializes a new instance of <see cref="UdpClientWrapper"/> class with specified <paramref name="destinationIpAddress"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// <remarks>If you don't know your real local IP address pass <paramref name="localIpAddress"/> as null.</remarks> 
        /// </summary>
        /// <param name="destinationIpAddress">A destination IP address.</param>
        /// <param name="port">The port used to send and receive datagrams.</param>
        /// <param name="localIpAddress">A local IP address.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destinationIpAddress"/> is null.</exception>
        protected UdpClientWrapper(IPAddress destinationIpAddress, int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (destinationIpAddress == null)
                throw new ArgumentNullException(nameof(destinationIpAddress));

            var localIp = localIpAddress ?? IPAddress.Any;
            _destinationEndpoint = new IPEndPoint(destinationIpAddress, port);

            UdpClient = new UdpClient();
            // allow multiple clients in the same PC
            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            var localEndpoint = new IPEndPoint(localIp, port);
            UdpClient.Client.Bind(localEndpoint);
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
        
        /// Calls <see cref="Send(byte[], IPEndPoint)"/> with <see cref="IPEndPoint"/> specified by destination IP address.
        /// <remarks>Use it for broadcast or multicast data sends.</remarks>
        public virtual IAsyncResult Send(byte[] data)
        {
            return Send(data, _destinationEndpoint);
        }

        /// <summary>
        /// Sends data to destination endpoint.
        /// </summary>
        /// <param name="data">/The data to be sent.</param>
        /// <param name="destinationEndpoint">Destination endpoint for sent data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <returns>An <see cref="IAsyncResult"/> object that references the asynchronous send.</returns>
        public virtual IAsyncResult Send(byte[] data, IPEndPoint destinationEndpoint)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Sent data is null or empty.");

            return CallUdpClientBeginSend(data, destinationEndpoint);
        }
        
        /// <summary>
        /// Closes udp client.
        /// </summary>
        public virtual void Close()
        {
            UdpClient.Close();
        }
        
        #endregion public methods

        #region private methods

        /// <summary>
        /// Invokes datagram receiving.
        /// </summary>
        /// <returns>An <see cref="IAsyncResult"/> object that references the asynchronous receive.</returns>
        private IAsyncResult CallUdpClientBeginReceive() => UdpClient.BeginReceive(ReceivedCallback, state: null);

        /// <summary>
        /// Ends pending asynchronous receive.
        /// </summary>
        /// <param name="beginReceiveResult">returned object by a call of UdpClient.BeginReceive</param>
        /// <returns></returns>
        private (byte[] receivedData, IPEndPoint senderEndpoint) EndReceive(IAsyncResult beginReceiveResult)
        {
            // _beginReceived = false;
            var senderEndpoint = new IPEndPoint(0, 0);
            var receivedData = UdpClient.EndReceive(beginReceiveResult, ref senderEndpoint);
            return (receivedData, senderEndpoint);
        }
        
        /// <summary>
        /// Invokes when datagram received. Fires <see cref="DatagramReceived"/> event.
        /// </summary>
        /// <param name="result">An object returned by a call of _client.BeginReceive.</param>
        private void ReceivedCallback(IAsyncResult result)
        {
            var (receivedData, senderEndpoint) = EndReceive(result);

            // trigger event if subscribed 
            DatagramReceived?.Invoke(this, new DatagramReceivedEventArgs(receivedData, senderEndpoint));
            // restart listening for UDP incomes
            CallUdpClientBeginReceive();
        }
        
        /// <summary>
        /// Invokes when datagram sent.
        /// </summary>
        /// <param name="beginSendResult">An object returned by a call of _client.BeginSend.</param>
        private void SentCallback(IAsyncResult beginSendResult)
        {
            UdpClient.EndSend(beginSendResult);
        }

        /// <summary>
        /// Invokes datagram sending.
        /// </summary>
        /// <param name="datagram">Datagram to be sent.</param>
        /// <param name="dstEp">Destination for sent datagram.</param>
        /// <returns>An <see cref="IAsyncResult"/> object that references the asynchronous send.</returns>
        private IAsyncResult CallUdpClientBeginSend(byte[] datagram, IPEndPoint dstEp) =>
            UdpClient.BeginSend(datagram, datagram.Length, dstEp, SentCallback, state: null);
        
        #endregion private methods
        
        /// <summary>
        /// Event occuring when datagram received.
        /// </summary>
        public event EventHandler<DatagramReceivedEventArgs> DatagramReceived;
    }
}