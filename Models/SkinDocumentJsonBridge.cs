using System.Drawing;
using SkinCreator.Json;
using SkinCreator.Models.Elements;

namespace SkinCreator.Models;

static class SkinDocumentJsonBridge
{
	// JSON POCO → SkinDocument
	public static SkinDocument FromJson(SkinJsonRoot root, Dictionary<string, Bitmap> imageCache)
	{
		var doc = new SkinDocument
		{
			Name        = root.Meta?.Name        ?? string.Empty,
			Author      = root.Meta?.Author      ?? string.Empty,
			Description = root.Meta?.Description ?? string.Empty,
			TransparentKey = root.Settings?.TransparentKey ?? "202030",
		};

		if (root.Images != null)
		{
			foreach (var kv in root.Images)
			{
				doc.Images[kv.Key] = new ImageEntry
				{
					Key          = kv.Key,
					RelativePath = kv.Value,
					Bitmap       = imageCache.TryGetValue(kv.Key, out var bmp) ? bmp : null,
				};
			}
		}

		if (root.MainForm != null)
			doc.MainForm = ConvertMainForm(root.MainForm);

		if (root.SubForms != null)
		{
			foreach (var kv in root.SubForms)
				doc.SubForms[kv.Key] = ConvertSubForm(kv.Key, kv.Value);
		}

		return doc;
	}

	// SkinDocument → JSON POCO
	public static SkinJsonRoot ToJson(SkinDocument doc)
	{
		var root = new SkinJsonRoot
		{
			Meta = new SkinMetaJson
			{
				Name        = doc.Name,
				Author      = doc.Author,
				Description = doc.Description,
			},
			Settings = new SkinSettingsJson { TransparentKey = doc.TransparentKey },
			Images   = doc.Images.ToDictionary(kv => kv.Key, kv => kv.Value.RelativePath),
			MainForm = ConvertMainFormToJson(doc.MainForm),
		};

		if (doc.SubForms.Count > 0)
		{
			root.SubForms = [];
			foreach (var kv in doc.SubForms)
				root.SubForms[kv.Key] = ConvertSubFormToJson(kv.Value);
		}

		return root;
	}

	// ── JSON → Model ──

	static FormDef ConvertMainForm(MainFormJson j)
	{
		var f = new FormDef
		{
			FormKey       = "MainForm",
			BackgroundSrc = ConvertSpriteRect(j.Src),
			Location      = ConvertLocation(j.Location) ?? new(),
		};

		if (j.Buttons != null)
			foreach (var kv in j.Buttons)
				f.Buttons.Add(ConvertButton(kv.Key, kv.Value));

		if (j.Sliders != null)
			foreach (var kv in j.Sliders)
				f.Sliders.Add(ConvertSlider(kv.Key, kv.Value));

		if (j.Labels != null)
			foreach (var kv in j.Labels)
				f.Labels.Add(ConvertLabel(kv.Key, kv.Value));

		if (j.Spectrum != null)
			f.Spectrum = ConvertSpectrum(j.Spectrum);

		if (j.WaveArea != null)
			f.WaveArea = ConvertWaveArea(j.WaveArea);

		if (j.Pictures != null)
			foreach (var kv in j.Pictures)
				f.Pictures.Add(ConvertPicture(kv.Key, kv.Value));

		return f;
	}

	static FormDef ConvertSubForm(string key, SubFormJson j)
	{
		var f = new FormDef
		{
			FormKey       = key,
			BackgroundSrc = ConvertSpriteRect(j.Src),
			Location      = ConvertLocation(j.Location) ?? new(),
			Offset        = ConvertLocation(j.Offset),
			Magnetic      = j.Magnetic,
			BackColor     = j.BackColor,
			ForeColor     = j.ForeColor,
			Font          = j.Font,
			FontSize      = j.FontSize,
		};

		if (j.Buttons != null)
			foreach (var kv in j.Buttons)
				f.Buttons.Add(ConvertButton(kv.Key, kv.Value));

		if (j.Labels != null)
			foreach (var kv in j.Labels)
				f.Labels.Add(ConvertLabel(kv.Key, kv.Value));

		if (j.Sliders != null)
			foreach (var kv in j.Sliders)
				f.Sliders.Add(ConvertSlider(kv.Key, kv.Value));

		if (j.Grids != null)
			foreach (var kv in j.Grids)
				f.Grids.Add(ConvertGrid(kv.Key, kv.Value));

		if (j.Pictures != null)
			foreach (var kv in j.Pictures)
				f.Pictures.Add(ConvertPicture(kv.Key, kv.Value));

		return f;
	}

	static ButtonElement ConvertButton(string key, ButtonJson j) => new()
	{
		ElementKey = key,
		Location   = ConvertLocation(j.Location) ?? new(),
		IsDisabled = j.IsDisabled,
		Up         = ConvertSpriteRect(j.Up),
		Down       = ConvertSpriteRect(j.Down),
		Hover      = ConvertSpriteRect(j.Hover),
		Disabled   = ConvertSpriteRect(j.Disabled),
		Optional   = ConvertSpriteRect(j.Optional),
	};

	static SliderElement ConvertSlider(string key, SliderJson j) => new()
	{
		ElementKey  = key,
		Location    = ConvertLocation(j.Location) ?? new(),
		IsDisabled  = j.IsDisabled,
		Src         = ConvertSpriteRect(j.Src),
		Min         = j.Min,
		Max         = j.Max,
		Orientation = j.Orientation ?? "horizontal",
	};

	static LabelElement ConvertLabel(string key, LabelJson j) => new()
	{
		ElementKey   = key,
		Location     = ConvertLocation(j.Location) ?? new(),
		IsDisabled   = j.IsDisabled,
		Font         = j.Font ?? "Arial",
		Size         = j.Size > 0 ? j.Size : 9,
		Bold         = j.Bold,
		Italic       = j.Italic,
		ForeColor    = j.ForeColor ?? "FFFFFF",
		BackColor    = j.BackColor ?? "Transparent",
		Align        = NormalizeAlign(j.Align),
		ScrollEnable = j.ScrollEnable,
		ScrollVector = j.ScrollVector,
		Interval     = j.Interval > 0 ? j.Interval : 50,
		AdditionalValue = j.AdditionalValue,
	};

	static SpectrumElement ConvertSpectrum(SpectrumJson j) => new()
	{
		ElementKey = "spectrum",
		Src        = ConvertSpriteRect(j.Src),
		Location   = ConvertLocation(j.Location) ?? new(),
		IsDisabled = j.IsDisabled,
		Color      = j.Color ?? "000000",
		WaveColorL = j.WaveColorL ?? "00FF00",
		WaveColorR = j.WaveColorR ?? "0000FF",
	};

	static WaveAreaElement ConvertWaveArea(WaveAreaJson j) => new()
	{
		ElementKey    = "waveArea",
		Location      = ConvertLocation(j.Location) ?? new(),
		IsDisabled    = j.IsDisabled,
		Target        = j.Target ?? "trackbar",
		Mode          = j.Mode ?? "normal",
		Exponent      = j.Exponent > 0 ? j.Exponent : 1.0f,
		ColorL        = j.ColorL ?? "FF0000",
		ColorR        = j.ColorR ?? "0000FF",
		ColorMix      = j.ColorMix ?? "FF00FF",
		ColorPlayed   = j.ColorPlayed ?? "00FF00",
		ColorUnplayed = j.ColorUnplayed ?? "202020",
	};

	static PictureElement ConvertPicture(string key, PictureJson j) => new()
	{
		ElementKey  = key,
		Location    = ConvertLocation(j.Location) ?? new(),
		IsDisabled  = j.IsDisabled,
		Src         = ConvertSpriteRect(j.Src),
		Color       = j.Color ?? "000000",
		BorderColor = j.BorderColor,
		BorderWidth = j.BorderWidth,
		CornerRadius = j.CornerRadius,
	};

	static GridElement ConvertGrid(string key, GridJson j) => new()
	{
		ElementKey      = key,
		Location        = ConvertLocation(j.Location) ?? new(),
		IsDisabled      = j.IsDisabled,
		BackColor       = j.BackColor ?? "1A1A2E",
		ForeColor       = j.ForeColor ?? "CCCCCC",
		LineColor       = j.LineColor,
		HeaderBackColor = j.HeaderBackColor,
		HeaderForeColor = j.HeaderForeColor,
		HeaderLineColor = j.HeaderLineColor,
	};

	static SpriteRectDef? ConvertSpriteRect(SpriteRectJson? j)
	{
		if (j == null) return null;
		return new SpriteRectDef { ImageKey = j.ImageKey, X = j.X, Y = j.Y, W = j.W, H = j.H };
	}

	static LocationDef? ConvertLocation(LocationJson? j)
	{
		if (j == null) return null;
		return new LocationDef { X = j.X, Y = j.Y, W = j.W, H = j.H };
	}

	// ── Model → JSON ──

	static MainFormJson ConvertMainFormToJson(FormDef f)
	{
		var j = new MainFormJson
		{
			Src      = ToSpriteRectJson(f.BackgroundSrc),
			Location = ToLocationJson(f.Location),
		};

		if (f.Buttons.Count > 0)
			j.Buttons = f.Buttons.ToDictionary(b => b.ElementKey, ToButtonJson);
		if (f.Sliders.Count > 0)
			j.Sliders = f.Sliders.ToDictionary(s => s.ElementKey, ToSliderJson);
		if (f.Labels.Count > 0)
			j.Labels = f.Labels.ToDictionary(l => l.ElementKey, ToLabelJson);
		if (f.Spectrum != null)
			j.Spectrum = ToSpectrumJson(f.Spectrum);
		if (f.WaveArea != null)
			j.WaveArea = ToWaveAreaJson(f.WaveArea);
		if (f.Pictures.Count > 0)
			j.Pictures = f.Pictures.ToDictionary(p => p.ElementKey, ToPictureJson);

		return j;
	}

	static SubFormJson ConvertSubFormToJson(FormDef f)
	{
		var j = new SubFormJson
		{
			Src       = ToSpriteRectJson(f.BackgroundSrc),
			Location  = ToLocationJson(f.Location),
			Offset    = ToLocationJson(f.Offset),
			Magnetic  = f.Magnetic,
			BackColor = f.BackColor,
			ForeColor = f.ForeColor,
			Font      = f.Font,
			FontSize  = f.FontSize,
		};

		if (f.Buttons.Count > 0)
			j.Buttons = f.Buttons.ToDictionary(b => b.ElementKey, ToButtonJson);
		if (f.Labels.Count > 0)
			j.Labels = f.Labels.ToDictionary(l => l.ElementKey, ToLabelJson);
		if (f.Sliders.Count > 0)
			j.Sliders = f.Sliders.ToDictionary(s => s.ElementKey, ToSliderJson);
		if (f.Grids.Count > 0)
			j.Grids = f.Grids.ToDictionary(g => g.ElementKey, ToGridJson);
		if (f.Pictures.Count > 0)
			j.Pictures = f.Pictures.ToDictionary(p => p.ElementKey, ToPictureJson);

		return j;
	}

	static ButtonJson ToButtonJson(ButtonElement b) => new()
	{
		Up       = ToSpriteRectJson(b.Up),
		Down     = ToSpriteRectJson(b.Down),
		Hover    = ToSpriteRectJson(b.Hover),
		Disabled = ToSpriteRectJson(b.Disabled),
		Optional = ToSpriteRectJson(b.Optional),
		Location = ToLocationJson(b.Location),
		IsDisabled = b.IsDisabled,
	};

	static SliderJson ToSliderJson(SliderElement s) => new()
	{
		Src         = ToSpriteRectJson(s.Src),
		Location    = ToLocationJson(s.Location),
		Min         = s.Min,
		Max         = s.Max,
		Orientation = s.Orientation,
		IsDisabled  = s.IsDisabled,
	};

	static LabelJson ToLabelJson(LabelElement l) => new()
	{
		Location     = ToLocationJson(l.Location),
		Font         = l.Font,
		Size         = l.Size,
		Bold         = l.Bold,
		Italic       = l.Italic,
		ForeColor    = l.ForeColor,
		BackColor    = l.BackColor,
		Align        = l.Align,
		ScrollEnable = l.ScrollEnable,
		ScrollVector = l.ScrollVector,
		Interval     = l.Interval,
		AdditionalValue = l.AdditionalValue,
		IsDisabled   = l.IsDisabled,
	};

	static SpectrumJson ToSpectrumJson(SpectrumElement s) => new()
	{
		Src        = ToSpriteRectJson(s.Src),
		Location   = ToLocationJson(s.Location),
		Color      = s.Color,
		WaveColorL = s.WaveColorL,
		WaveColorR = s.WaveColorR,
		IsDisabled = s.IsDisabled,
	};

	static WaveAreaJson ToWaveAreaJson(WaveAreaElement w) => new()
	{
		Location      = ToLocationJson(w.Location),
		Target        = w.Target,
		Mode          = w.Mode,
		Exponent      = w.Exponent,
		ColorL        = w.ColorL,
		ColorR        = w.ColorR,
		ColorMix      = w.ColorMix,
		ColorPlayed   = w.ColorPlayed,
		ColorUnplayed = w.ColorUnplayed,
		IsDisabled    = w.IsDisabled,
	};

	static PictureJson ToPictureJson(PictureElement p) => new()
	{
		Src         = ToSpriteRectJson(p.Src),
		Location    = ToLocationJson(p.Location),
		Color       = p.Color,
		BorderColor = p.BorderColor,
		BorderWidth = p.BorderWidth,
		CornerRadius = p.CornerRadius,
		IsDisabled  = p.IsDisabled,
	};

	static GridJson ToGridJson(GridElement g) => new()
	{
		Location        = ToLocationJson(g.Location),
		BackColor       = g.BackColor,
		ForeColor       = g.ForeColor,
		LineColor       = g.LineColor,
		HeaderBackColor = g.HeaderBackColor,
		HeaderForeColor = g.HeaderForeColor,
		HeaderLineColor = g.HeaderLineColor,
		IsDisabled      = g.IsDisabled,
	};

	static SpriteRectJson? ToSpriteRectJson(SpriteRectDef? s)
	{
		if (s == null) return null;
		return new SpriteRectJson { ImageKey = s.ImageKey, X = s.X, Y = s.Y, W = s.W, H = s.H };
	}

	static LocationJson? ToLocationJson(LocationDef? l)
	{
		if (l == null) return null;
		return new LocationJson { X = l.X, Y = l.Y, W = l.W, H = l.H };
	}

	static string NormalizeAlign(string? align) =>
		align?.Trim().ToLowerInvariant() switch
		{
			"center" => "center",
			"right" => "right",
			_ => "left",
		};
}
