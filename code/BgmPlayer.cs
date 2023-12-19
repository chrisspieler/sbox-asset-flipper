using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class BgmPlayer : Component
{
	[Property] public int MaxConcurrentTracks { get; set; } = 1;

	private List<BgmTrack> _allTracks = new();
	private List<BgmTrack> _trackList = new();
	private List<MusicPlayer> _musicPlayers = new();

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
		while ( _musicPlayers.Count >= MaxConcurrentTracks )
		{
			var oldestTrack = _musicPlayers[0];
			oldestTrack.Stop();
			_musicPlayers.RemoveAt( 0 );
		}
		var musicPlayer = MusicPlayer.Play( FileSystem.Mounted, track.AudioTrack );
		musicPlayer.ListenLocal = true;
		musicPlayer.Volume = 0.2f * track.Volume;
		musicPlayer.OnFinished = () =>
		{
			Log.Info( "FINISHED" );
			_musicPlayers.Remove( musicPlayer );
			PlayNext();
		};
		_musicPlayers.Add( musicPlayer );
	}
}
