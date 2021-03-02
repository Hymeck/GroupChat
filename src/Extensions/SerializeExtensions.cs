using System;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace GroupChat.Extensions
{
    public static class SerializeExtensions
    {
        public static byte[] XmlSerialize<TValue>(this TValue source) where TValue : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var serializer = new XmlSerializer(typeof(TValue));
            using var memStream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(memStream);
            serializer.Serialize(xmlWriter, source);
            
            return memStream.ToArray();
        }

        public static TValue XmlDeserialize<TValue>(this byte[] sourceBytes) where TValue : class
        {
            if (sourceBytes == null)
                throw new ArgumentNullException(nameof(sourceBytes));

            if (sourceBytes.Length == 0)
                throw new InvalidOperationException(nameof(sourceBytes));
            
            var serializer = new XmlSerializer(typeof(TValue));

            using var memStream = new MemoryStream(sourceBytes);
            using var xmlReader = XmlReader.Create(memStream);
            
            return serializer.Deserialize(xmlReader) as TValue;
        }
        
        public static byte[] JsonSerialize<TValue>(this TValue source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return JsonSerializer.SerializeToUtf8Bytes(source);
        }
        
        public static TValue JsonDeserialize<TValue>(this byte[] sourceBytes)
        {
            if (sourceBytes == null)
                throw new ArgumentNullException(nameof(sourceBytes));

            return JsonSerializer.Deserialize<TValue>(sourceBytes);
        }
    }
}