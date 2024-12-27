using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace add_video_to_youtube_playlist
{
    internal class PlaylistManager
    {
        private const int MaxResults = 20;  // Max query results when getting collections.
        private YouTubeService _youtubeService;

        public PlaylistManager(YouTubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        internal async Task<Google.Apis.YouTube.v3.Data.Channel> GetChannelAsync(string channelId)
        {
            Console.WriteLine($"Retrieving channel {channelId}");

            var request = _youtubeService.Channels.List("id,snippet");
            request.MaxResults = MaxResults;
            request.Id = channelId;
            var response = await request.ExecuteAsync();

            var channel = response.Items.SingleOrDefault();
            if (channel != null)
            {
                return channel;
            }

            throw new Exception("Channel not found");
        }

        /// <summary>
        /// Get a playlist's ID from its title
        /// </summary>
        internal async Task<string> GetPlaylistIdByTitleAsync(string playlistTitle)
        {
            Console.WriteLine($"Retrieving playlist {playlistTitle}");

            var request = _youtubeService.Playlists.List("id,snippet");
            request.Mine = true;
            request.MaxResults = MaxResults;
            var response = await request.ExecuteAsync();

            foreach (var playlist in response.Items)
            {
                if (playlist.Snippet.Title == playlistTitle)
                {
                    return playlist.Id;
                }
            }

            throw new Exception("Playlist not found");
        }

        /// <summary>
        /// Get the last {channelMaxResults} video IDs from the specified channel
        /// </summary>
        internal async Task<List<string>> GetVideoIdsByChannelIdAsync(string channelId, int channelMaxResults)
        {
            var request = _youtubeService.Search.List("id, snippet");
            request.ChannelId = channelId;
            request.Type = "video";
            request.MaxResults = channelMaxResults;
            request.Order = SearchResource.ListRequest.OrderEnum.Date;
            var response = await request.ExecuteAsync();

            var videoIds = new List<string>();
            foreach (var searchResult in response.Items)
            {
                videoIds.Add(searchResult.Id.VideoId);
            }

            return videoIds;
        }

        /// <summary>
        /// Add a video to a playlist
        /// </summary>
        /// <param name="videoId">The ID of the video</param>
        /// <param name="playlistId">The ID of the playlist</param>
        /// <param name="playlistName">The playlist name for display purposes only</param>
        /// <returns></returns>
        internal async Task AddVideoToPlaylistAsync(string videoId, string playlistId, string playlistName)
        {
            // Query the playlist for the specified video
            var playlistItemsRequest = _youtubeService.PlaylistItems.List("id,snippet");
            playlistItemsRequest.PlaylistId = playlistId;
            playlistItemsRequest.VideoId = videoId;

            var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
            if (playlistItemsResponse != null
                && playlistItemsResponse.Items != null
                && playlistItemsResponse.Items.Count == 0)
            {
                // Add video if it isn't already in the list
                try
                {
                    var newPlaylistItem = CreateNewPlaylistItem(videoId, playlistId);
                    var playlistItemsInsertRequest = _youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet");
                    var playlistItemsInsertResponse = await playlistItemsInsertRequest.ExecuteAsync();

                    Console.WriteLine($"Video {playlistItemsInsertResponse.Snippet.Title} added to playlist {playlistName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add video to playlist: {ex.Message}");
                }
            }
            else
            {
                // Video is already in the list
                Console.WriteLine($"Video {videoId} is already in playlist {playlistName}");
            }
        }

        /// <summary>
        /// Create a playlist item from a video ID and playlist ID
        /// </summary>
        private PlaylistItem CreateNewPlaylistItem(string videoId, string playlistId)
        {
            // Create new playlist item
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlistId;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = videoId;

            return newPlaylistItem;
        }
    }
}
