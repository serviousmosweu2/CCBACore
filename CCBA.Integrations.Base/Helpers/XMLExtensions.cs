using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CCBA.Integrations.Base.Helpers
{
    public static class XMLExtensions
    {
        /// <summary>
        /// Developer: Johan Nieuwenhuis, Dattatray Mharanur
        /// </summary>
        /// <param name="dmfXmlFiles"></param>
        /// <returns></returns>
        public static string CreatePackage(Dictionary<string, string> dmfXmlFiles, string source)
        {
            var tempPath = Path.GetTempPath();
            var target = $@"{tempPath}{Guid.NewGuid()}.zip";
            File.Copy(source, target);

            foreach (var (key, value) in dmfXmlFiles)
            {
                var filename = $"{key}.xml";
                File.WriteAllText($@"{tempPath}{filename}", value);
                using var fileStream = new FileStream(target, FileMode.Open);
                using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update);
                zipArchive.CreateEntryFromFile($@"{tempPath}{filename}", filename);
            }

            return target;
        }

        /// <summary>
        /// Developer: Johan Nieuwenhuis, Dattatray Mharanur
        /// </summary>
        /// <param name="dmfXmlFiles"></param>
        /// <returns></returns>
        public static async Task<byte[]> CreatePackage(this Dictionary<string, string> dmfXmlFiles)
        {
            await using var outStream = new MemoryStream();
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Update, true))
            {
                foreach (var (key, value) in dmfXmlFiles)
                {
                    await using var zipStream = archive.CreateEntry(key, CompressionLevel.Optimal).Open();
                    await using var fileToCompressStream = new MemoryStream(Encoding.Default.GetBytes(value));
                    await fileToCompressStream.CopyToAsync(zipStream);
                }
            }
            return outStream.ToArray();
        }

        /// <summary>
        /// Developer: Johan Nieuwenhuis, Dattatray Mharanur, Konrad Steynberg
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<byte[]> CreatePackage(this Dictionary<string, byte[]> data)
        {
            using var outStream = new MemoryStream();
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Update, true))
            {
                foreach (var (key, value) in data)
                {
                    await using var zipStream = archive.CreateEntry(key, CompressionLevel.Optimal).Open();
                    await using var fileToCompressStream = new MemoryStream(value);
                    await fileToCompressStream.CopyToAsync(zipStream);
                }
            }
            return outStream.ToArray();
        }

        /// <summary>
        /// Developer: Johan Nieuwenhuis, Dattatray Mharanur
        /// </summary>
        /// <param name="dmfXmlFiles"></param>
        /// <returns></returns>
        public static async Task<string> CreatePackageAsync(Dictionary<string, string> dmfXmlFiles, string source)
        {
            var tempPath = Path.GetTempPath();
            var target = $@"{tempPath}{Guid.NewGuid()}.zip";
            File.Copy(source, target);

            foreach (var (key, value) in dmfXmlFiles)
            {
                var filename = $"{key}.xml";
                await File.WriteAllTextAsync($@"{tempPath}{filename}", value);
                await using var fileStream = new FileStream(target, FileMode.Open);
                using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update);
                zipArchive.CreateEntryFromFile($@"{tempPath}{filename}", filename);
            }

            return target;
        }

        public static T Deserialize<T>(this string obj) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(obj);
            return (T)serializer.Deserialize(stringReader);
        }

        [Obsolete("Use Deserialize")]
        public static Task<T> DeSerialize<T>(this string obj) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(obj);
            return Task.FromResult((T)serializer.Deserialize(stringReader));
        }

        public static string DictionaryToXml(this List<Dictionary<string, string>> dic, string entityName, string parent = "Document")
        {
            var list = new XElement(parent);
            foreach (var dictionary in dic)
            {
                list.Add(new XElement(entityName, dictionary.Where(s => !string.IsNullOrWhiteSpace(s.Value)).Select(kv => new XElement(kv.Key.Trim(), kv.Value.Trim())).ToList()));
            }

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + list;
        }

        /// <summary>
        /// Warning: This also checks whether the xml document is a valid dataset
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static bool IsValidXml(this string xml)
        {
            var xDocument = XDocument.Parse(xml);
            if (xDocument.Elements().First().Elements().Count() == 1)
            {
                return !xDocument.Elements().First().Elements().First().Elements().Select(x => x.Value).All(string.IsNullOrEmpty);
            }
            return true;
        }

        [Obsolete("Use IsValidXml")]
        public static bool IsValidXML(this string xml)
        {
            return IsValidXml(xml);
        }

        public static string Serialize<T>(this T obj) where T : new()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.Default))
                xmlSerializer.Serialize(streamWriter, obj, ns);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }

        public static async Task<string> SerializeAsync<T>(this T obj) where T : new()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var xmlSerializer = new XmlSerializer(typeof(T));
            await using var memoryStream = new MemoryStream();
            await using (var streamWriter = new StreamWriter(memoryStream, Encoding.Default))
                xmlSerializer.Serialize(streamWriter, obj, ns);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }

        public static async Task<string> SerializeAsync<T>(this List<T> data)
        {
            if (data.Count == 0) return null;
            var xmlSerializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute("Document"));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            await using var memoryStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            xmlSerializer.Serialize(streamWriter, data, ns);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }

        public static string SerializeList<T>(this List<T> data)
        {
            if (data.Count == 0) return null;
            var xmlSerializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute("Document"));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            xmlSerializer.Serialize(streamWriter, data, ns);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }

        public static async Task<string> SerializeListAsync<T>(this List<T> attachmentsList)
        {
            if (attachmentsList.Count == 0) return null;
            var xmlSerializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute("Document"));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            await using var memoryStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            xmlSerializer.Serialize(streamWriter, attachmentsList, ns);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }

        public static Dictionary<string, string> XMLToDictionary(this string xml)
        {
            return XElement.Parse(xml).Elements().ToDictionary(el => el.Name.LocalName, el => el.Value);
        }
    }
}