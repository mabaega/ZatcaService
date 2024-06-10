using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace ZatcaService.Helpers
{
    public static class DocumentFormatter
    {
        public static string Beautify(this XDocument doc)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the XDocument to the MemoryStream using UTF-8 encoding
                using (XmlWriter writer = XmlWriter.Create(memoryStream, new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = Encoding.UTF8 // Set encoding to UTF-8
                }))
                {
                    doc.Save(writer);
                }

                // Reset the position of the MemoryStream
                memoryStream.Position = 0;

                // Read the MemoryStream using StreamReader with UTF-8 encoding
                using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }


}
