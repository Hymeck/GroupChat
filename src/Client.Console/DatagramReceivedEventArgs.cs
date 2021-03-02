using System;

namespace GroupChat.Client.Console
{
    public class DatagramReceivedEventArgs : EventArgs
    {
        public byte[] Datagram { get; init; }

        public DatagramReceivedEventArgs(byte[] datagram) => Datagram = datagram;
    }
}