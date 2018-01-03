using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace WammpCommons.Business
{
    public class ServiceUpdater
    {
        public const int REQUEST_TIMEOUT_MS = 4000;

        public class VersionInfo
        {
            public Version LatestVersion { get; set; }
            public string LatestVersionUrl { get; set; }        
        }

        public ServiceUpdater()
        {

        }

        public ServiceUpdater(IWebProxy proxy) : base()
        {
            this.Proxy = proxy;
        }

        public IWebProxy Proxy { get; set; }

        public VersionInfo GetMetaInfoVersion(string url)
        {
            VersionInfo version = null;
            string contents = null;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = Proxy;
            request.Timeout = REQUEST_TIMEOUT_MS;

            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    contents = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (string.IsNullOrEmpty(contents) == false)
            {
                XmlDocument xmldoc = new XmlDocument();

                xmldoc.LoadXml(contents);

                version = new VersionInfo();

                string latestversion = xmldoc.SelectSingleNode("//latestversion").InnerText;
                version.LatestVersionUrl = xmldoc.SelectSingleNode("//latestversionurl").InnerText;

                version.LatestVersion = new Version(latestversion);
            }

            return version;
        }
    }
}
