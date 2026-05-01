namespace SkinCreator.Models.Elements;

class PictureElement : SkinElementBase
{
	public override ElementType Type => ElementType.Picture;
	public SpriteRectDef? Src { get; set; }
	public string Color { get; set; } = "000000";
	public string? BorderColor { get; set; }
	public int BorderWidth { get; set; }
	public int CornerRadius { get; set; }
}
