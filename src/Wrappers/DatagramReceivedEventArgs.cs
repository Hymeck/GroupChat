namespace GroupChat.Shared.Wrappers
{
    public class DatagramReceivedEventArgs : System.EventArgs
    {
        public byte[] Datagram { get; init; }

        public DatagramReceivedEventArgs(byte[] datagram) => Datagram = datagram;
    }
}