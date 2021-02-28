using System;

namespace Entry
{
    public class DatagramReceivedEventArgs : EventArgs
    {
        public byte[] Datagram { get; init; }

        public DatagramReceivedEventArgs(byte[] datagram) => Datagram = datagram;
    }
}