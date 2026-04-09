using SkinCreator.Models;
using SkinCreator.Models.Elements;

namespace SkinCreator.Commands;

class MoveElementCommand : IEditorCommand
{
	readonly ISkinElement _element;
	readonly LocationDef _before;
	readonly LocationDef _after;

	public string Description => "移動";

	public MoveElementCommand(ISkinElement element, LocationDef before, LocationDef after)
	{
		_element = element;
		_before = before.Clone();
		_after = after.Clone();
	}

	public void Execute()
	{
		_element.Location.X = _after.X;
		_element.Location.Y = _after.Y;
		_element.Location.W = _after.W;
		_element.Location.H = _after.H;
	}

	public void Undo()
	{
		_element.Location.X = _before.X;
		_element.Location.Y = _before.Y;
		_element.Location.W = _before.W;
		_element.Location.H = _before.H;
	}
}
