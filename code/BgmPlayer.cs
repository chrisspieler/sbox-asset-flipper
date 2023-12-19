using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class BgmPlayer : Component
{
	[Property] public float PlaybackPosition => _musicPlayer?.PlaybackTime ?? 0;
	[Property] public float Volume => _musicPlayer?.Volume ?? 0;
	[Property] public string Track => _trackInfo?.Title;
	[Property] public string Artist => _trackInfo?.Artist;
	[Property] public string License => _trackInfo?.License;


	private List<BgmTrack> _allTracks = new();
	private List<BgmTrack> _trackList = new();
	private MusicPlayer _musicPlayer;
	private BgmTrack _trackInfo;

	protected override void OnStart()
	{
		_allTracks = ResourceLibrary.GetAll<BgmTrack>().ToList();
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
		if ( !_trackList.Any() )
		{
			_trackList = _allTracks.OrderBy( _ => Guid.NewGuid() ).ToList();
		}
		if ( !_trackList.Any() )
		{
			Log.Error( "No BGM tracks loaded." );
			return;
		}
		var track = _trackList[0];
		_trackList.RemoveAt( 0 );
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
