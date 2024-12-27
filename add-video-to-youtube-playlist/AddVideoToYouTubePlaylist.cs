using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace add_video_to_youtube_playlist
{
    internal class AddVideoToYouTubePlaylist
    {
        private PlaylistManager? _playlistManager;

        internal static async Task RunAsync(IEnumerable<Channel> channels)
        {
            var addVideoToYouTubePlaylist = new AddVideoToYouTubePlaylist();
            await addVideoToYouTubePlaylist.InitCredentialAsync();
            await addVideoToYouTubePlaylist.ExecuteAsync(channels);
        }

        internal async Task InitCredentialAsync()
        {
            // 1. Create credential
            UserCredential credential;
            using (var stream = new FileStream("C:/Source/credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore("AddVideoToPlaylist"));
            }

            // 2. Create a YouTubeService with the authenticated client
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AddVideoToPlaylist"
            });
            _playlistManager = new PlaylistManager(youtubeService);

            Console.WriteLine("Credential authorised");
        }

        internal async Task ExecuteAsync(IEnumerable<Channel> channels)
        {

            foreach (var channel in channels)
            {
                var playlistName = channel.Playlist;
                var playlist = await _playlistManager!.GetPlaylistIdByTitleAsync(playlistName);
                var youTubeChannel = await _playlistManager!.GetChannelAsync(channel.Id);
                var videos = await _playlistManager!.GetVideoIdsByChannelIdAsync(youTubeChannel.Id, channel.MaxResults);
                foreach (var video in videos)
                {
                    await _playlistManager!.AddVideoToPlaylistAsync(video, playlist, playlistName);
                }
            }
        }
    }
}
