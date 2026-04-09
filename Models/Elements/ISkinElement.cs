namespace SkinCreator.Models.Elements;

enum ElementType { Button, Slider, Label, Spectrum, WaveArea, Picture, Grid }

interface ISkinElement
{
	string ElementKey { get; set; }
	ElementType Type { get; }
	LocationDef Location { get; set; }
	bool IsDisabled { get; set; }
}
