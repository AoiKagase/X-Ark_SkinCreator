namespace SkinCreator.Models.Elements;

class WaveAreaElement : SkinElementBase
{
	public override ElementType Type => ElementType.WaveArea;
	public string Target { get; set; } = "trackbar";
	public string Mode { get; set; } = "mix";
	public float Exponent { get; set; } = 2.5f;
	public string ColorL { get; set; } = "00CC66";
	public string ColorR { get; set; } = "0066CC";
	public string ColorMix { get; set; } = "00AA88";
	public string ColorPlayed { get; set; } = "555555";
	public string ColorUnplayed { get; set; } = "333333";
}
