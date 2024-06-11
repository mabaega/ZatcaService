using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace ZatcaService.Helpers
{
    public static class DocumentFormatter
    {
        public static string Beautify(this XDocument doc)
        {
            using MemoryStream memoryStream = new();
            using (XmlWriter writer = XmlWriter.Create(memoryStream, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = Encoding.UTF8
            }))
            {
                doc.Save(writer);
            }

            memoryStream.Position = 0;
            using StreamReader reader = new(memoryStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }


}
