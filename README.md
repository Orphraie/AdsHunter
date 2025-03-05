AdsHunter is a .NET application that acts as a MITM proxy to intercept and block online advertisements.

The key functionality is implemented below:

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


This asynchronous method intercepts HTTP responses before they reach the client. It checks the request's URL and headers to identify ad content from specific sources.
For Twitch, if the URL matches the ad endpoint, the response body is replaced with an empty string, effectively blocking the ad.
For YouTube, it examines both the URL and header for indicators of an ad (like "&ctier=" or "ad-id="), and similarly clears the response content if an ad is detected.
