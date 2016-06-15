using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using YoutubeExtractor;

namespace YoutubePlaylistToMp3Album
{
    [Activity(Label = "YoutubePlaylistToMp3Album", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private readonly WebClient _client = new WebClient();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate
            {
                string playlistHtml = GetPlaylistHtmlFile("https://www.youtube.com/playlist?list=PLXxpXtJUHTPFlS6dHpsst4BcVnqqPL0tk");
                string albumName = "Tech Trance/Harder Trance/Techno";

                IEnumerable<string> videoIds = GetVideoIdsFromPlaylistHtml(playlistHtml);

                foreach (var videoId in videoIds)
                {
                    DownloadAndProcessAudio(videoId, albumName);
                }
            };
        }

        private void DownloadAndProcessAudio(string videoId, string albumName)
        {
            string filePath = DownloadAudio(videoId, albumName);
            ProcessAudio(filePath);
        }

        private void ProcessAudio(string filePath)
        {
            throw new NotImplementedException();
        }

        private string DownloadAudio(string videoId, string albumName)
        {
            string location = "/storage/external_SD/Muzika";
            string youtubeLink =
                "https://www.youtube.com/watch?v=" + videoId;

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(youtubeLink);

            VideoInfo video = videoInfos
                .Where(info => info.AudioType == AudioType.Aac)
                .OrderByDescending(info => info.AudioBitrate)
                .FirstOrDefault();

            if (video == null)
            {
                throw new NullReferenceException("Audio was not found");
            }

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            var downloadUrl = video.DownloadUrl;

            return "";
        }

        private IEnumerable<string> GetVideoIdsFromPlaylistHtml(string playlistHtml)
        {
            HashSet<string> result = new HashSet<string>();


            int index = playlistHtml.IndexOf("data-video-ids=", StringComparison.Ordinal);

            while (index != -1)
            {
                var start = index + 16;
                const int length = 11;


                string videoId = playlistHtml.Substring(start, length);

                if (!result.Contains(videoId) && videoId != "\"__VIDEO_ID")
                    result.Add(videoId);

                index = playlistHtml.IndexOf("data-video-ids=", start+11, StringComparison.Ordinal);
            }

            return result;
        }

        private string GetPlaylistHtmlFile(string url)
        {
            string html = _client.DownloadString(url);
            return html;
        }
    }
}

