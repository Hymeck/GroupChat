using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GroupChat.Extensions
{
    public static class UdpClientExtensions
    {
        public static async Task<int> SendAsync(this UdpClient udpClient, byte[] datagram) => 
            await udpClient.SendAsync(datagram, datagram.Length, (IPEndPoint)udpClient.Client.RemoteEndPoint);

        public static async Task<UdpReceiveResult> ReceiveAsync(this UdpClient udpClient, int timeout, CancellationToken token)
        {
            var receiveTask = udpClient.ReceiveAsync();
            var timeoutTask = Task.Delay(timeout, token);
 
            await Task.WhenAny(receiveTask, timeoutTask);
 
            return receiveTask.IsCompleted 
                ? receiveTask.Result 
                : new UdpReceiveResult();
        }
    }
}