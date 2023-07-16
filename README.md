# add-video-to-youtube-playlist
Add video(s) to YouTube Playlists from your YouTube Subscriptions

# Overview

I'm a big consumer of YouTube, and found myself manually adding videos from my subscriptions into various playlists for later consumption. There's no reason this should be a manual process, so I automated it.

# Running

In order to run the solution, you will need two things.

1. A credentials file in `C:/Source/credentials.json` for your YouTube API account.
2. A channels file in `C:/Source/channels.json` listing your subscriptions and the corresponding target playlists.

## Credentials file

Here's how to get your credentials file [TODO]

## Channels file 

This file directs the app to add the specified number of videos from the specified subscription to the specified playlist. Note the playlist must already exist. Note this reads videos from the subscription by date descending.

Fields:
```
Name: The name of the subscription
Playlist: The playlist to add the videos to
MaxResults: The number of videos to add from the subscription
```

Example:
```
[
  {
    "Name": "Fireship",
    "Playlist": "Tech",
    "MaxResults": 10
  },
  {
    "Name": "Beyond Fireship",
    "Playlist": "Tech",
    "MaxResults": 10
  },
  {
    "Name": "Tiago Catarino",
    "Playlist": "LEGO",
    "MaxResults": 10
  }
]
```
In the example above, the most recent 10 videos from the channel `Fireship` will be added to the playlist `Tech`.
