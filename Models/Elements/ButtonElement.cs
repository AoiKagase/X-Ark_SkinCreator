namespace SkinCreator.Models.Elements;

class ButtonElement : SkinElementBase
{
	public override ElementType Type => ElementType.Button;
	public SpriteRectDef? Up { get; set; }
	public SpriteRectDef? Down { get; set; }
	public SpriteRectDef? Hover { get; set; }
	public SpriteRectDef? Disabled { get; set; }
	public SpriteRectDef? Optional { get; set; }
}
