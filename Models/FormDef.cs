using SkinCreator.Models.Elements;

namespace SkinCreator.Models;

class FormDef
{
	public string FormKey { get; set; } = "MainForm";
	public SpriteRectDef? BackgroundSrc { get; set; }
	public LocationDef Location { get; set; } = new();

	// サブフォーム固有
	public LocationDef? Offset { get; set; }
	public bool Magnetic { get; set; }
	public string? BackColor { get; set; }
	public string? ForeColor { get; set; }
	public string? Font { get; set; }
	public int FontSize { get; set; }

	public List<ButtonElement> Buttons { get; set; } = [];
	public List<SliderElement> Sliders { get; set; } = [];
	public List<LabelElement> Labels { get; set; } = [];
	public SpectrumElement? Spectrum { get; set; }
	public WaveAreaElement? WaveArea { get; set; }
	public List<PictureElement> Pictures { get; set; } = [];
	public List<GridElement> Grids { get; set; } = [];

	public IEnumerable<ISkinElement> AllElements()
	{
		foreach (var b in Buttons) yield return b;
		foreach (var s in Sliders) yield return s;
		foreach (var l in Labels) yield return l;
		if (Spectrum != null) yield return Spectrum;
		if (WaveArea != null) yield return WaveArea;
		foreach (var p in Pictures) yield return p;
		foreach (var g in Grids) yield return g;
	}
}
