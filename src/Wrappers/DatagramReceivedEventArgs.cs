using System;
using System.Net;
namespace GroupChat.Shared.Wrappers
{
    public class DatagramReceivedEventArgs : EventArgs
    {
        public byte[] Datagram { get; init; }
        public IPEndPoint From { get; init; }

        public DatagramReceivedEventArgs(byte[] datagram, IPEndPoint from) => (Datagram, From) = (datagram, from);
    }
}