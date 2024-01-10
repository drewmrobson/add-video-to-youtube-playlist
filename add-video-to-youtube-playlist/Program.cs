using add_video_to_youtube_playlist;

var channels = System.Text.Json.JsonSerializer.Deserialize<List<Channel>>(File.ReadAllText(@"C:/Source/channels.json"));
AddVideoToYouTubePlaylist.RunAsync(channels).Wait();
