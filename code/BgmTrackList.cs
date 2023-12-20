using Sandbox;
using System.Collections.Generic;

[GameResource("BGM Track List", "tracklst", "A list of BGM tracks.", Icon = "playlist_add_check_circle" )]
public class BgmTrackList : GameResource
{
	public List<BgmTrack> Tracks { get; set; }
}
