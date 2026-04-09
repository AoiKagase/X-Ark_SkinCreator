using SkinCreator.Models;
using SkinCreator.Models.Elements;

namespace SkinCreator.Commands;

class AddElementCommand : IEditorCommand
{
	readonly FormDef _form;
	readonly ISkinElement _element;

	public string Description => $"追加: {_element.ElementKey}";

	public AddElementCommand(FormDef form, ISkinElement element)
	{
		_form = form;
		_element = element;
	}

	public void Execute() => AddToForm(_form, _element);
	public void Undo() => RemoveFromForm(_form, _element);

	static void AddToForm(FormDef form, ISkinElement element)
	{
		switch (element)
		{
			case ButtonElement b: form.Buttons.Add(b); break;
			case SliderElement s: form.Sliders.Add(s); break;
			case LabelElement l: form.Labels.Add(l); break;
			case SpectrumElement sp: form.Spectrum = sp; break;
			case WaveAreaElement w: form.WaveArea = w; break;
			case PictureElement p: form.Pictures.Add(p); break;
			case GridElement g: form.Grids.Add(g); break;
		}
	}

	static void RemoveFromForm(FormDef form, ISkinElement element)
	{
		switch (element)
		{
			case ButtonElement b: form.Buttons.Remove(b); break;
			case SliderElement s: form.Sliders.Remove(s); break;
			case LabelElement l: form.Labels.Remove(l); break;
			case SpectrumElement: form.Spectrum = null; break;
			case WaveAreaElement: form.WaveArea = null; break;
			case PictureElement p: form.Pictures.Remove(p); break;
			case GridElement g: form.Grids.Remove(g); break;
		}
	}
}
