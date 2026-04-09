namespace SkinCreator.Commands;

class ChangePropertyCommand : IEditorCommand
{
	readonly Action _redo;
	readonly Action _undo;

	public string Description { get; }

	public ChangePropertyCommand(string description, Action redo, Action undo)
	{
		Description = description;
		_redo = redo;
		_undo = undo;
	}

	public void Execute() => _redo();
	public void Undo() => _undo();
}
