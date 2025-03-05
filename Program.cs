using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AdsHunter
{
    class Program
    {

        // Import WinAPI functions to control the console window.
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        static NotifyIcon trayIcon;
        static MyMitmProxy proxy;

        static void Main()
        {
            //
            // Hide the console window.
            //
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            // Set up the tray icon.
            trayIcon = new NotifyIcon
            {
                Icon    = SystemIcons.Application,
                Text    = "AdsHunter",
                Visible = true
            };

            //
            // Create a context menu for the tray icon.
            //
            var contextMenu     = new ContextMenuStrip();
            var exitMenuItem    = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (sender, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            };

            contextMenu.Items.Add(exitMenuItem);
            trayIcon.ContextMenuStrip = contextMenu;

            var pfxPath     = "MyLocalRootCA.pfx";
            var pfxPassword = "MonPass";
            
            if (!File.Exists(pfxPath))
            {
                CreateRootCA(pfxPath, pfxPassword);
            }
            
            var rootCert = new X509Certificate2(pfxPath, pfxPassword, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            InstallCertificateInRootStore(rootCert);
            
            proxy = new MyMitmProxy();
            proxy.Start();

            Application.ApplicationExit += OnApplicationExit;
            Application.Run();
        }

        /// <summary>
        /// Handles cleanup when the application is exiting.
        /// </summary>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            // Stop the proxy server.
            proxy.Stop();

            // Hide the tray icon.
            trayIcon.Visible = false;
        }

        /// <summary>
        /// Creates a self-signed Root CA certificate and saves it as a .PFX file.
        /// </summary>
        static void CreateRootCA(string pfxPath, string password)
        {
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest("CN=MyMITMProxyRootCA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.DigitalSignature, true));
                request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

                var rootCert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));

                File.WriteAllBytes(pfxPath, rootCert.Export(X509ContentType.Pfx, password));
            }
        }

        /// <summary>
        /// Installs a certificate into the root store.
        /// </summary>
        static void InstallCertificateInRootStore(X509Certificate2 cert)
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);

                if (!store.Certificates.Contains(cert))
                {
                    store.Add(cert);
                }

                store.Close();
            }
        }
    }
}