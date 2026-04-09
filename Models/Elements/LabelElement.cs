namespace SkinCreator.Models.Elements;

class LabelElement : SkinElementBase
{
	public override ElementType Type => ElementType.Label;
	public string Font { get; set; } = "Arial";
	public int Size { get; set; } = 9;
	public bool Bold { get; set; }
	public bool Italic { get; set; }
	public string ForeColor { get; set; } = "FFFFFF";
	public string BackColor { get; set; } = "000000";
	public bool ScrollEnable { get; set; }
	public int ScrollVector { get; set; } = 3;
	public int Interval { get; set; } = 50;
	public string? AdditionalValue { get; set; }
}
