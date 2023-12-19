using Sandbox;

[GameResource("BGM Track", "bgm", "Background music with credits.", Icon = "music_note")]
public class BgmTrack : GameResource
{
	public string AudioTrack { get; set; }
	public float Volume { get; set; } = 1.0f;
	public string Title { get; set; }
	public string Artist { get; set; }
	public string License { get; set; }
}
