namespace SkinCreator.Commands;

interface IEditorCommand
{
	string Description { get; }
	void Execute();
	void Undo();
}
