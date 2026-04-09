namespace SkinCreator.Models;

class SpriteRectDef
{
	public string? ImageKey { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
	public int W { get; set; }
	public int H { get; set; }

	public bool IsEmpty => string.IsNullOrEmpty(ImageKey);

	public System.Drawing.Rectangle ToRectangle() => new(X, Y, W, H);

	public SpriteRectDef Clone() => new() { ImageKey = ImageKey, X = X, Y = Y, W = W, H = H };
}
