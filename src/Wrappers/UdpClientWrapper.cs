using System;
using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Provides methods for sending and receiving datagrams asynchronously.
    /// </summary>
    public class UdpClientWrapper : BaseUdpClientWrapper
    {
        /// <summary>
        /// Indicates whether listening of receiving data has begun or not.
        /// </summary>
        protected bool _beginReceived;
        
        /// <inheritdoc/>
        public UdpClientWrapper(IPAddress destinationIpAddress, int port, IPAddress localIpAddress = null) : 
            base(destinationIpAddress, port, localIpAddress)
        {
        }
        
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
        /// Disposes <see cref="System.Net.Sockets.UdpClient"/> object.
        /// </summary>
        public virtual void Dispose()
        {
            UdpClient.Dispose();
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