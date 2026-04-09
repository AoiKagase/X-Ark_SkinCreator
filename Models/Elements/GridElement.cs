namespace SkinCreator.Models.Elements;

class GridElement : SkinElementBase
{
	public override ElementType Type => ElementType.Grid;
	public string BackColor { get; set; } = "1A1A2E";
	public string ForeColor { get; set; } = "CCCCCC";
	public string? LineColor { get; set; }
	public string? HeaderBackColor { get; set; }
	public string? HeaderForeColor { get; set; }
	public string? HeaderLineColor { get; set; }
}
