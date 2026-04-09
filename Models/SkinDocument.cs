using SkinCreator.Commands;
using SkinCreator.Models.Elements;

namespace SkinCreator.Models;

class SkinDocument : IDisposable
{
	public string Name { get; set; } = "New Skin";
	public string Author { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string TransparentKey { get; set; } = "202030";

	public Dictionary<string, ImageEntry> Images { get; set; } = [];
	public FormDef MainForm { get; set; } = new() { FormKey = "MainForm", Location = new() { W = 579, H = 174 } };
	public Dictionary<string, FormDef> SubForms { get; set; } = [];

	public bool IsDirty { get; set; }
	public string? CurrentFilePath { get; set; }
	public string? TempDirectory { get; set; }

	readonly Stack<IEditorCommand> _undoStack = new();
	readonly Stack<IEditorCommand> _redoStack = new();

	public bool CanUndo => _undoStack.Count > 0;
	public bool CanRedo => _redoStack.Count > 0;

	public string? UndoDescription => _undoStack.TryPeek(out var cmd) ? cmd.Description : null;
	public string? RedoDescription => _redoStack.TryPeek(out var cmd) ? cmd.Description : null;

	public void Execute(IEditorCommand command)
	{
		command.Execute();
		_undoStack.Push(command);
		_redoStack.Clear();
		IsDirty = true;
	}

	public void Undo()
	{
		if (!CanUndo)
			return;
		var cmd = _undoStack.Pop();
		cmd.Undo();
		_redoStack.Push(cmd);
		IsDirty = true;
	}

	public void Redo()
	{
		if (!CanRedo)
			return;
		var cmd = _redoStack.Pop();
		cmd.Execute();
		_undoStack.Push(cmd);
		IsDirty = true;
	}

	public FormDef GetForm(string formKey)
	{
		if (formKey == "MainForm")
			return MainForm;
		return SubForms.TryGetValue(formKey, out var f) ? f : MainForm;
	}

	public System.Drawing.Bitmap? GetBitmap(string? imageKey)
	{
		if (string.IsNullOrEmpty(imageKey))
			return null;
		return Images.TryGetValue(imageKey, out var entry) ? entry.Bitmap : null;
	}

	public System.Drawing.Bitmap? CropBitmap(SpriteRectDef? src)
	{
		if (src == null || src.IsEmpty)
			return null;
		var bmp = GetBitmap(src.ImageKey);
		if (bmp == null)
			return null;
		if (src.W == 0 || src.H == 0)
			return new System.Drawing.Bitmap(bmp);
		var rect = src.ToRectangle();
		if (rect.X < 0 || rect.Y < 0 || rect.Right > bmp.Width || rect.Bottom > bmp.Height)
			return new System.Drawing.Bitmap(bmp);
		var cropped = new System.Drawing.Bitmap(rect.Width, rect.Height);
		using var g = System.Drawing.Graphics.FromImage(cropped);
		g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height), rect, System.Drawing.GraphicsUnit.Pixel);
		return cropped;
	}

	public IEnumerable<ISkinElement> GetAllElements(string formKey)
		=> GetForm(formKey).AllElements();

	public void Dispose()
	{
		foreach (var entry in Images.Values)
			entry.Dispose();
		Images.Clear();

		if (TempDirectory != null && Directory.Exists(TempDirectory))
		{
			try { Directory.Delete(TempDirectory, true); }
			catch { }
		}
	}
}
