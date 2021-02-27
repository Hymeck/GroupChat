using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Entry
{
    public static class SerializeExtensions
    {
        public static byte[] Serialize<TClass>(this TClass source) where TClass : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var serializer = new XmlSerializer(typeof(TClass));
            using var memStream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(memStream);
            serializer.Serialize(xmlWriter, source);
            return memStream.ToArray();
        }

        public static TClass Deserialize<TClass>(this byte[] sourceBytes) where TClass : class
        {
            if (sourceBytes == null)
                throw new ArgumentNullException(nameof(sourceBytes));

            if (sourceBytes.Length == 0)
                throw new InvalidOperationException(nameof(sourceBytes));
            
            var serializer = new XmlSerializer(typeof(TClass));

            using var memStream = new MemoryStream(sourceBytes);
            using var xmlReader = XmlReader.Create(memStream);
            return serializer.Deserialize(xmlReader) as TClass;
        }
    }
}