using System.Linq;

namespace GroupChat.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] PrependByte(this byte[] source, byte prependByte) =>
            source
                .Prepend(prependByte)
                .ToArray();
    }
}