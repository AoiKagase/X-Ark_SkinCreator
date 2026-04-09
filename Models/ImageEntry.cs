using System.Drawing;

namespace SkinCreator.Models;

class ImageEntry : IDisposable
{
	public string Key { get; set; } = string.Empty;
	public string RelativePath { get; set; } = string.Empty;
	public string? AbsolutePath { get; set; }
	public Bitmap? Bitmap { get; set; }

	public void Dispose()
	{
		Bitmap?.Dispose();
		Bitmap = null;
	}
}
