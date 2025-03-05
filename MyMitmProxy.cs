using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace AdsHunter
{
    public class MyMitmProxy
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        const int INTERNET_OPTION_REFRESH = 37;

        private ProxyServer proxyServer;
        private ExplicitProxyEndPoint endPoint;

        public MyMitmProxy()
        {
            string proxy = "http://127.0.0.1:8888";
            string proxyConfig = "PROXY " + proxy;

            // Update Registry
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                                              "ProxyEnable", 1);
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                                              "ProxyServer", proxy);

            // Apply changes system-wide
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            proxyServer = new ProxyServer();

            // ✅ Ensure a Root Certificate exists (creates one if missing)
            proxyServer.CertificateManager.EnsureRootCertificate();

            // ✅ Assign it automatically
            proxyServer.CertificateManager.RootCertificateIssuerName = "MyMITMProxy";

            // ✅ Create a Proxy endpoint that supports HTTPS
            endPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8888, true);
            proxyServer.AddEndPoint(endPoint);

            // ✅ Enable HTTP/2 Support
            proxyServer.EnableHttp2 = true;

            // ✅ Allow forwarding to real websites
            proxyServer.ForwardToUpstreamGateway = true;

            // ✅ Subscribe to events
            proxyServer.BeforeRequest += OnBeforeRequest;
            proxyServer.BeforeResponse += OnBeforeResponse;
        }

        /// <summary>
        /// 🚀 Generates a new 2048-bit RSA Root Certificate
        /// </summary>
        public static X509Certificate2 GenerateRootCertificate(ProxyServer proxyServer)
        {
            var certManager = proxyServer.CertificateManager;

            // ✅ Ensure the Root Certificate is generated and trusted
            certManager.EnsureRootCertificate();

            // ✅ Retrieve and return the Root Certificate
            return certManager.RootCertificate;
        }

        public void Start()
        {
            proxyServer.Start();

            //Console.WriteLine("✅ Proxy is running on port 8888");
        }

        public void Stop()
        {
            proxyServer.BeforeRequest  -= OnBeforeRequest;
            proxyServer.BeforeResponse -= OnBeforeResponse;
            proxyServer.Stop();

            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 0);
            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);

            //Console.WriteLine("🔴 Proxy stopped.");
        }

        private async Task OnBeforeRequest(object sender, SessionEventArgs e)
        {
            
        }

        private async Task OnBeforeResponse(object sender, SessionEventArgs e)
        {
            var header = e.HttpClient.Request.HeaderText;
            var body   = await e.GetResponseBodyAsString();

            var url = e.HttpClient.Request.Url;

            //
            // 🔍 Twitch Ads
            //
            if (url.Contains("https://edge.ads.twitch.tv/ads"))
            {
                e.SetResponseBodyString(string.Empty);

                //Console.WriteLine("🚫 Blocking Twitch Ad Video...");
            }

            //
            // 🔍 Youtube Ads
            //
            if (url.Contains("googlevideo.com/videoplayback"))
            {
                if (header.Contains("&ctier=") || header.Contains("ad-id="))
                {
                    e.SetResponseBodyString(string.Empty);

                    //Console.WriteLine("🚫 Blocking Youtube Ad Video...");
                }
            }
        }
    }
}
