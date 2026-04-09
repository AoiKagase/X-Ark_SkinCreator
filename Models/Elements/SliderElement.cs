namespace SkinCreator.Models.Elements;

class SliderElement : SkinElementBase
{
	public override ElementType Type => ElementType.Slider;
	public SpriteRectDef? Src { get; set; }
	public int Min { get; set; }
	public int Max { get; set; } = 100;
	public string Orientation { get; set; } = "horizontal";
}
