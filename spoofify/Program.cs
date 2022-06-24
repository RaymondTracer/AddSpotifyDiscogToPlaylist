using SpotifyAPI.Web;

// Get token from https://developer.spotify.com/console/post-playlist-tracks/
string Token = "";

// Get market code from https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2
string Market = "";

// Specify ID's here:
string Playlist = "";
string Artist = "";

// The API client itself.
SpotifyClient Client = new(Token);

// Magic starts here.
int albumOffset = 0;
ArtistsAlbumsRequest albReq = new()
{
    IncludeGroupsParam = ArtistsAlbumsRequest.IncludeGroups.Album | ArtistsAlbumsRequest.IncludeGroups.Single | ArtistsAlbumsRequest.IncludeGroups.Compilation,
    Limit = 50,
    Market = Market,
    Offset = albumOffset
};
Paging<SimpleAlbum> albums = await Client.Artists.GetAlbums(Artist, albReq);

List<string> songs = new();
if (albums.Total > 0 && albums.Items != null)
{
    Console.WriteLine($"Found {albums.Total} album{S(albums.Total)}.");
    Console.WriteLine();

    while (albumOffset < albums.Total)
    {
        foreach (SimpleAlbum album in albums.Items)
        {
            Console.WriteLine(album.Name);

            if (album.TotalTracks > 0)
            {
                int trackOffset = 0;

                while (trackOffset < album.TotalTracks)
                {
                    AlbumTracksRequest traReq = new()
                    {
                        Limit = 50,
                        Market = Market,
                        Offset = trackOffset
                    };

                    Paging<SimpleTrack> tracks = await Client.Albums.GetTracks(album.Id);

                    if (tracks.Items != null)
                    {
                        foreach (SimpleTrack t in tracks.Items)
                        {
                            Console.WriteLine($" {t.Name}");
                            songs.Add(t.Uri);
                        }
                    }
                    else
                    {
                        Console.WriteLine($" {album.TotalTracks} track{S(album.TotalTracks)} found but list is null. (wtf?!?!?!?)");
                    }

                    trackOffset += 50;
                }
            }
            else
            {
                Console.WriteLine(" No tracks found.");
            }
        }

        albumOffset += 50;
    }

    Console.WriteLine($"Total songs: {songs.Count}");
    Console.WriteLine();

    int songOffset = 0;
    while (albumOffset < songs.Count)
    {
        Console.WriteLine($"{songOffset}/{songs.Count}");
        await AddSongs(songs.Subset(songOffset, 100));
        songOffset += 100;
    }
}
else
{
    Console.WriteLine("No albums found.");
}

async Task AddSongs(List<string> songs) // Max 100 at a time
{
    await Client.Playlists.AddItems(Playlist, new(songs));
}

string S(int? amount)
{
    return amount != 1 ? "s" : "";
}

public static class Ext
{
    public static List<T> Subset<T>(this List<T> list, int offset, int amount)
    {
        List<T> temp = new();

        for (int i = offset; i < offset + amount; i++)
        {
            if (i >= list.Count) break;
            temp.Add(list[i]);
        }

        return temp;
    }
}