using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using CosmosHttp.Client;
using System.Net;

namespace StarOS.Network
{
    public static class Http
    {
        public static byte[] DownloadRawFile(string url)
        {
            if (url.StartsWith("https://"))
            {
                throw new WebException("HTTPS currently not supported, please use http://");
            }

            string path = ExtractPathFromUrl(url);
            string domainName = ExtractDomainNameFromUrl(url);

            var dnsClient = new DnsClient();

            dnsClient.Connect(DNSConfig.DNSNameservers[0]);
            dnsClient.SendAsk(domainName);
            Address address = dnsClient.Receive();
            dnsClient.Close();

            HttpRequest request = new HttpRequest
            {
                IP = address.ToString(),
                Domain = domainName,
                Path = path,
                Method = "GET"
            };
            request.Send();

            return request.Response.GetStream();
        }

        public static string DownloadFile(string url)
        {
            if (url.StartsWith("https://"))
            {
                throw new WebException("HTTPS currently not supported, please use http://");
            }

            string path = ExtractPathFromUrl(url);
            string domainName = ExtractDomainNameFromUrl(url);

            var dnsClient = new DnsClient();

            dnsClient.Connect(DNSConfig.DNSNameservers[0]);
            dnsClient.SendAsk(domainName);
            Address address = dnsClient.Receive();
            dnsClient.Close();

            HttpRequest request = new HttpRequest
            {
                IP = address.ToString(),
                Domain = domainName,
                Path = path,
                Method = "GET"
            };
            request.Send();

            return request.Response.Content;
        }

        private static string ExtractDomainNameFromUrl(string url)
        {
            int start = url.Contains("://") ? url.IndexOf("://") + 3 : 0;
            int end = url.IndexOf("/", start);
            if (end == -1) end = url.Length;
            return url[start..end];
        }

        private static string ExtractPathFromUrl(string url)
        {
            int start = url.Contains("://") ? url.IndexOf("://") + 3 : 0;
            int indexOfSlash = url.IndexOf("/", start);
            return indexOfSlash != -1 ? url.Substring(indexOfSlash) : "/";
        }
    }
}
