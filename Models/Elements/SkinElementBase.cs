namespace SkinCreator.Models.Elements;

abstract class SkinElementBase : ISkinElement
{
	public string ElementKey { get; set; } = string.Empty;
	public abstract ElementType Type { get; }
	public LocationDef Location { get; set; } = new();
	public bool IsDisabled { get; set; }
}
