﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace add_video_to_youtube_playlist
{
    internal class AddVideoToYouTubePlaylist
    {
        private const int MaxResults = 20;  // Max query results when getting collections.

        private YouTubeService? _youtubeService;

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
                        new FileDataStore("YouTubeIntegrationDemo"));
            }

            // 2. Create a YouTubeService with the authenticated client
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YouTubeIntegrationDemo"
            });

            Console.WriteLine("Credential authorised");
        }

        internal async Task ExecuteAsync(IEnumerable<Channel> channels)
        {
            // 1. Get all subscriptions
            var channelNames = channels.Select(x => x.Name).ToArray();
            var subscriptions = await GetSubscriptionsAsync(channelNames);

            foreach (var channel in channels)
            {
                var channelName = channel.Name;
                var playlistName = channel.Playlist;

                Console.WriteLine($"Adding {channelName} to {playlistName}");

                // 2. Get the relevant playlist
                var playlist = await GetPlaylistIdByTitleAsync(playlistName);

                // 3. Get videos from channel
                var subscription = subscriptions.FirstOrDefault(x => x.Snippet.Title == channelName);
                if (subscription == null)
                {
                    Console.WriteLine($"0 Videos for {channelName}");
                    continue;
                }

                var videos = await GetVideoIdsByChannelIdAsync(subscription.Snippet.ResourceId.ChannelId, channel.MaxResults);
                foreach (var video in videos)
                {
                    // 4. Add video to playlist
                    await AddVideoToPlaylistAsync(video, playlist, playlistName);
                }
            }
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
        /// Get all subscriptions for the current user
        /// </summary>
        internal async Task<IList<Subscription>> GetSubscriptionsAsync(string[] channels)
        {
            Console.WriteLine("Retrieving subscriptions");

            List<Subscription> result = new List<Subscription>();

            string nextpagetoken = " ";
            while (nextpagetoken != null)
            {
                var subscriptionsListRequest = _youtubeService.Subscriptions.List("snippet,contentDetails");
                //subscriptionsListRequest.ChannelId = userId;
                subscriptionsListRequest.Mine = true;
                subscriptionsListRequest.MaxResults = MaxResults;
                subscriptionsListRequest.PageToken = nextpagetoken;
                subscriptionsListRequest.Order = SubscriptionsResource.ListRequest.OrderEnum.Unread;
                var subscriptionsListResponse = await subscriptionsListRequest.ExecuteAsync();

                foreach (var subscription in subscriptionsListResponse.Items)
                {
                    foreach (var channelList in channels)
                    {
                        if (channelList.Contains(subscription.Snippet.Title))
                        {
                            result.Add(subscription);
                        }
                    }
                }

                nextpagetoken = subscriptionsListResponse.NextPageToken;
            }

            return result;
        }

        /// <summary>
        /// Get the last channelMaxResults videos from the specified playlist
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
        /// Add a specific video to a specific playlist
        /// </summary>
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

        internal PlaylistItem CreateNewPlaylistItem(string videoId, string playlistId)
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