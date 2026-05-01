using System.Drawing;

namespace SkinCreator.Helpers;

static class ColorHelper
{
	public static Color FromHex(string? hex, Color fallback = default)
	{
		if (string.IsNullOrEmpty(hex))
			return fallback == default ? Color.Black : fallback;
		try
		{
			hex = hex.Trim();
			if (string.Equals(hex, "Transparent", StringComparison.OrdinalIgnoreCase))
				return Color.Transparent;

			hex = hex.TrimStart('#');
			if (hex.Length == 6)
			{
				int win32 = Convert.ToInt32(hex, 16);
				return ColorTranslator.FromWin32(win32);
			}
		}
		catch { }
		return fallback == default ? Color.Black : fallback;
	}

	public static string ToHex(Color color) =>
		$"{ColorTranslator.ToWin32(color) & 0xFFFFFF:X6}";
}
