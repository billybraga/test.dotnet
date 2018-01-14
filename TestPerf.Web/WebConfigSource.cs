using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace TestPerf.Web
{
    public class WebConfigSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new WebConfigConfigurationProvider(this);
        }

        public class WebConfigConfigurationProvider : FileConfigurationProvider
        {
            public WebConfigConfigurationProvider(WebConfigSource source) : base(source) { }

            public override void Load(Stream stream)
            {
                Data = XDocument
                    .Load(stream)
                    .Element("configuration")
                    .Element("appSettings")
                    .Elements("add")
                    .ToDictionary(
                        _ => "webconfig:" + _.Attribute("key").Value.Replace(".", string.Empty), 
                        _ => _.Attribute("value").Value
                    );
            }
        }
    }
}