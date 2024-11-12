# add-video-to-youtube-playlist

Add video(s) to YouTube Playlists from specific YouTube channels.

I built this dumb little app for myself and maybe you can find it useful too?

- [Overview](#overview)
- [Dependencies](#dependencies)
- [Usage](#usage)

## Overview

I'm a big consumer of YouTube, and found myself manually adding videos from my subscriptions into various playlists for later consumption. There's no reason this should be a manual process, so I automated it.

I then realised I don't have to be subscribed to all these channels, and made a simple list of channels to add videos from.

## Dependencies

### Credentials file

Follow [this guide](https://developers.google.com/workspace/guides/create-credentials#service-account) to create service account credentials. Save this file as `C:/Source/credentials.json`.

### Channels file

This file directs the app to add the specified number of videos from the specified channel to the specified playlist. Note the playlist must already exist. Note this reads videos from the channel by date descending.

Fields:

```text
Id: The YouTube Channel ID
Name: The name of the channel
Playlist: The playlist to add the videos to
MaxResults: The number of videos to add from the channel
```

Example:

```json
[
  {
    "Id": "UCsBjURrPoezykLs9EqgamOA",
    "Name": "Fireship",
    "Playlist": "Tech",
    "MaxResults": 10
  },
  {
    "Id": "UCsBjURrPoezykLs9EqgamOA",
    "Name": "Beyond Fireship",
    "Playlist": "Tech",
    "MaxResults": 10
  },
  {
    "Id": "UCsBjURrPoezykLs9EqgamOA",
    "Name": "Tiago Catarino",
    "Playlist": "LEGO",
    "MaxResults": 10
  }
]
```

In the example above, the 10 most recent videos from the channel `Fireship` will be added to the playlist `Tech`, the 10 most recent videos from the channel `Beyond Fireship` will be added to the playlist `Tech`, and the 10 most recent videos from the channel `Tiago Catarino` will be added to the playlist `LEGO`.

Save this file as `C:/Source/channels.json`.

## Usage

In order to run the solution, you will need two things.

1. A credentials file in `C:/Source/credentials.json` for your YouTube API account.
2. A channels file in `C:/Source/channels.json` listing channels and the corresponding target playlists.

Then follow these steps:

1. `git clone https://github.com/drewmrobson/add-video-to-youtube-playlist.git`
2. `cd add-video-to-youtube-playlist/add-video-to-youtube-playlist`
3. `dotnet run`
