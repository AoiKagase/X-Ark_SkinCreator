namespace SkinCreator.Models;

class LocationDef
{
	public int X { get; set; }
	public int Y { get; set; }
	public int W { get; set; }
	public int H { get; set; }

	public System.Drawing.Rectangle ToRectangle() => new(X, Y, W, H);

	public LocationDef Clone() => new() { X = X, Y = Y, W = W, H = H };
}
