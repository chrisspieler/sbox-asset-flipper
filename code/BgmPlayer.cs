using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class BgmPlayer : Component
{
	[Property] public BgmTrackList CurrentTrackList { get; set; }
	[Property] public float PlaybackPosition => _musicPlayer?.PlaybackTime ?? 0;
	[Property] public float Volume => _musicPlayer?.Volume ?? 0;
	[Property] public string Track => _trackInfo?.Title;
	[Property] public string Artist => _trackInfo?.Artist;
	[Property] public string License => _trackInfo?.License;


	private List<BgmTrack> _queue = new();
	private MusicPlayer _musicPlayer;
	private BgmTrack _trackInfo;

	protected override void OnStart()
	{
		PlayNext();
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "reload" ) )
		{
			PlayNext();
		}
	}

	public void PlayNext()
	{
		if ( CurrentTrackList is null || CurrentTrackList.Tracks?.Any() != true )
			return;

		if ( !_queue.Any() )
		{
			_queue = CurrentTrackList.Tracks.OrderBy( _ => Guid.NewGuid() ).ToList();
		}
		var track = _queue[0];
		_queue.RemoveAt( 0 );
		PlayTrack( track );
	}

	private void PlayTrack( BgmTrack track )
	{
		_musicPlayer?.Stop();
		var musicPlayer = MusicPlayer.Play( FileSystem.Mounted, track.AudioTrack );
		musicPlayer.ListenLocal = true;
		musicPlayer.Volume = 0.2f * track.Volume;
		musicPlayer.OnFinished = () =>
		{
			_musicPlayer?.Stop();
			_trackInfo = null;
			PlayNext();
		};
		_musicPlayer = musicPlayer;
		_trackInfo = track;
	}
}
