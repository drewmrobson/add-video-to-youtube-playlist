using add_video_to_youtube_playlist;
using System.Text.Json;

var channels = JsonSerializer.Deserialize<List<Channel>>(File.ReadAllText(@"C:/Source/channels.json"));
AddVideoToYouTubePlaylist.RunAsync(channels!).Wait();
