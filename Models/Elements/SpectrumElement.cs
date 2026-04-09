namespace SkinCreator.Models.Elements;

class SpectrumElement : SkinElementBase
{
	public override ElementType Type => ElementType.Spectrum;
	public SpriteRectDef? Src { get; set; }
	public string Color { get; set; } = "000000";
	public string WaveColorL { get; set; } = "00FF00";
	public string WaveColorR { get; set; } = "0000FF";
}
