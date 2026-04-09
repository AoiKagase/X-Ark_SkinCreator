using System.Drawing;

namespace SkinCreator.Helpers;

static class HandleRenderer
{
	const int HandleSize = 6;

	public static void DrawHandles(Graphics g, Rectangle rect)
	{
		using var pen = new Pen(Color.DodgerBlue, 1);
		using var brush = new SolidBrush(Color.White);

		g.DrawRectangle(pen, rect);

		foreach (var pt in GetHandlePoints(rect))
		{
			var hr = new Rectangle(pt.X - HandleSize / 2, pt.Y - HandleSize / 2, HandleSize, HandleSize);
			g.FillRectangle(brush, hr);
			g.DrawRectangle(pen, hr);
		}
	}

	public static Cursors.ResizeDirection HitTest(Rectangle rect, Point pt)
	{
		var points = GetHandlePoints(rect);
		string[] dirs = ["NW", "N", "NE", "W", "E", "SW", "S", "SE"];

		for (int i = 0; i < points.Length; i++)
		{
			var hr = new Rectangle(points[i].X - HandleSize / 2, points[i].Y - HandleSize / 2, HandleSize, HandleSize);
			if (hr.Contains(pt))
				return (Cursors.ResizeDirection)i;
		}
		if (rect.Contains(pt))
			return Cursors.ResizeDirection.Move;
		return Cursors.ResizeDirection.None;
	}

	static Point[] GetHandlePoints(Rectangle r) =>
	[
		new(r.Left, r.Top),
		new(r.Left + r.Width / 2, r.Top),
		new(r.Right, r.Top),
		new(r.Left, r.Top + r.Height / 2),
		new(r.Right, r.Top + r.Height / 2),
		new(r.Left, r.Bottom),
		new(r.Left + r.Width / 2, r.Bottom),
		new(r.Right, r.Bottom),
	];
}

static class Cursors
{
	public enum ResizeDirection { NW, N, NE, W, E, SW, S, SE, Move, None }

	public static System.Windows.Forms.Cursor GetCursor(ResizeDirection dir) => dir switch
	{
		ResizeDirection.NW or ResizeDirection.SE => System.Windows.Forms.Cursors.SizeNWSE,
		ResizeDirection.NE or ResizeDirection.SW => System.Windows.Forms.Cursors.SizeNESW,
		ResizeDirection.N  or ResizeDirection.S  => System.Windows.Forms.Cursors.SizeNS,
		ResizeDirection.W  or ResizeDirection.E  => System.Windows.Forms.Cursors.SizeWE,
		ResizeDirection.Move => System.Windows.Forms.Cursors.SizeAll,
		_ => System.Windows.Forms.Cursors.Default,
	};
}
