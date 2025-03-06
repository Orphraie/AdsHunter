🛠 AdsHunter is a .NET application that acts as a MITM (Man-In-The-Middle) proxy to intercept and block YouTube and Twitch video advertisements.

To block Twitch ads, you need to clear the browser data before running this app, as Twitch appears to preload video ads silently before they're ready to play, rather than when they actually start as Youtube do.

🔑 The key functionality is implemented below:

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
                //
                //Console.WriteLine("🚫 Blocking Twitch Ad Video...");
                //
                e.SetResponseBodyString(string.Empty);
            }

            //
            // 🔍 Youtube Ads
            //
            if (url.Contains("googlevideo.com/videoplayback"))
            {
                if (header.Contains("&ctier=") || header.Contains("ad-id="))
                {
                    //
                    //Console.WriteLine("🚫 Blocking Youtube Ad Video...");
                    //
                    e.SetResponseBodyString(string.Empty);
                }
            }
        }


This asynchronous method intercepts HTTP responses before they reach the client. It checks the request's URL and headers to identify ad content from specific sources.

☕ If you'd like to support this project, you can donate Bitcoin on Lightning network (BTC) to the following address:

`lnurl1dp68gurn8ghj7ampd3kx2ar0veekzar0wd5xjtnrdakj7tnhv4kxctttdehhwm30d3h82unvwqhkgctjd9hxwumpd3skgvec4sf9fk`

Thank you for your support ? 🪙
