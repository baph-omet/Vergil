using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Vergil.XML;

namespace Vergil.Web {
    /// <summary>
    /// Wrapper class for HttpWebRequest to make simple GET and POST requests less of a hassle. Seriously, why isn't this in .NET by default already?
    /// </summary>
    public static class HTTP {
        /// <summary>
        /// Post an XML file to a URL.
        /// </summary>
        /// <param name="url">The web address where this data will be received.</param>
        /// <param name="xml">The XMLFile to send.</param>
        /// <param name="credentials">Credential information.</param>
        /// <param name="certificate">Certificate information.</param>
        /// <returns>The response from the internal request.</returns>
        public static string Post(string url, XMLFile xml, NetworkCredential credentials = null, X509Certificate2 certificate = null) {
            return Post(url, xml.ToString(), "text/xml", credentials, certificate);
        }
        /// <summary>
        /// Post data as a string to a URL.
        /// </summary>
        /// <param name="url">The web address where this data will be received.</param>
        /// <param name="data">A string containing the data to post.</param>
        /// <param name="contentType">The content type. Default: "text/xml"</param>
        /// <param name="credentials">Credential information.</param>
        /// <param name="certificate">Certificate information.</param>
        /// <returns>The response from the internal request.</returns>
        public static string Post(string url, string data, string contentType = "text/xml", NetworkCredential credentials = null, X509Certificate2 certificate = null) {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(data);
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
            req.ContentType = contentType;
            req.ContentLength = bytes.Length;
            req.Method = "POST";
            if (credentials != null) req.Credentials = credentials;
            if (certificate != null) req.ClientCertificates.Add(certificate);

            using (Stream rs = req.GetRequestStream()) rs.Write(bytes, 0, bytes.Length);
            HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
            using (Stream rs = resp.GetResponseStream()) return new StreamReader(rs).ReadToEnd();
        }

        /// <summary>
        /// Get data from a URL.
        /// </summary>
        /// <param name="url">The web address from which data will be requested.</param>
        /// <param name="credentials">Credential information.</param>
        /// <param name="certificate">Certificate information.</param>
        /// <returns>The response from the internal request.</returns>
        public static string Get(string url, NetworkCredential credentials = null, X509Certificate2 certificate = null) {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
            req.Method = "GET";
            if (credentials != null) req.Credentials = credentials;
            if (certificate != null) req.ClientCertificates.Add(certificate);

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream rs = resp.GetResponseStream())
            using (StreamReader reader = new StreamReader(rs)) {
                return reader.ReadToEnd();
            }
        }
    }
}
