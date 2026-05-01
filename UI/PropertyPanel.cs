using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Helpers;
using SkinCreator.Models;
using SkinCreator.Models.Elements;

namespace SkinCreator.UI;

class PropertyPanel : ScrollableControl
{
	SkinDocument? _doc;
	FormDef? _form;
	ISkinElement? _current;
	public event EventHandler? ElementChanged;

	public PropertyPanel()
	{
		AutoScroll  = true;
		BackColor   = Color.FromArgb(25, 25, 38);
		ForeColor   = Color.FromArgb(200, 200, 220);
		Padding     = new Padding(8);
	}

	public void Bind(SkinDocument? doc, FormDef? form, ISkinElement? element)
	{
		_doc     = doc;
		_form    = form;
		_current = element;
		Rebuild();
	}

	void Rebuild()
	{
		SuspendLayout();
		Controls.Clear();
		int y = 8;

		if (_current == null)
		{
			if (_form != null)
				BuildFormProps(ref y, _form);
			ResumeLayout();
			return;
		}

		AddHeader(ref y, _current.ElementKey + " (" + _current.Type + ")");
		AddBoolField(ref y, "IsDisabled", _current.IsDisabled, v => { _current.IsDisabled = v; Notify(); });
		AddLocationSection(ref y, _current.Location, editableSize: _current is not ButtonElement);

		switch (_current)
		{
			case ButtonElement b:   BuildButtonProps(ref y, b);   break;
			case SliderElement s:   BuildSliderProps(ref y, s);   break;
			case LabelElement l:    BuildLabelProps(ref y, l);    break;
			case SpectrumElement sp: BuildSpectrumProps(ref y, sp); break;
			case WaveAreaElement w: BuildWaveAreaProps(ref y, w); break;
			case PictureElement p:  BuildPictureProps(ref y, p);  break;
			case GridElement g:     BuildGridProps(ref y, g);     break;
		}

		ResumeLayout();
	}

	void BuildButtonProps(ref int y, ButtonElement b)
	{
		AddSeparator(ref y, "UP 画像");
		AddSpriteRectSection(ref y, b.Up, s => { b.Up = s; Notify(); });

		AddSeparator(ref y, "DOWN 画像");
		AddSpriteRectSection(ref y, b.Down, s => { b.Down = s; Notify(); });

		AddSeparator(ref y, "HOVER 画像");
		AddSpriteRectSection(ref y, b.Hover, s => { b.Hover = s; Notify(); });

		AddSeparator(ref y, "無効時 画像");
		AddSpriteRectSection(ref y, b.Disabled, s => { b.Disabled = s; Notify(); });

		AddSeparator(ref y, "Optional 画像");
		AddSpriteRectSection(ref y, b.Optional, s => { b.Optional = s; Notify(); });
	}

	void BuildSliderProps(ref int y, SliderElement s)
	{
		AddSeparator(ref y, "画像");
		AddSpriteRectSection(ref y, s.Src, v => { s.Src = v; Notify(); });
		AddIntField(ref y, "Min", s.Min, 0, 10000, v => { s.Min = v; Notify(); });
		AddIntField(ref y, "Max", s.Max, 0, 10000, v => { s.Max = v; Notify(); });
		AddComboField(ref y, "Orientation", ["horizontal", "vertical"], s.Orientation,
			v => { s.Orientation = v; Notify(); });
	}

	void BuildLabelProps(ref int y, LabelElement l)
	{
		AddStringField(ref y, "Font", l.Font, v => { l.Font = v; Notify(); });
		AddIntField(ref y, "Size", l.Size, 6, 72, v => { l.Size = v; Notify(); });
		AddBoolField(ref y, "Bold", l.Bold, v => { l.Bold = v; Notify(); });
		AddBoolField(ref y, "Italic", l.Italic, v => { l.Italic = v; Notify(); });
		AddColorField(ref y, "ForeColor", l.ForeColor, v => { l.ForeColor = v; Notify(); });
		AddColorField(ref y, "BackColor", l.BackColor, v => { l.BackColor = v; Notify(); });
		AddBoolField(ref y, "ScrollEnable", l.ScrollEnable, v => { l.ScrollEnable = v; Notify(); });
		AddComboField(ref y, "ScrollVector", ["0", "1", "2", "3"], l.ScrollVector.ToString(),
			v => { if (int.TryParse(v, out var n)) { l.ScrollVector = n; Notify(); } });
		AddIntField(ref y, "Interval (ms)", l.Interval, 10, 1000, v => { l.Interval = v; Notify(); });
		AddStringField(ref y, "AdditionalValue", l.AdditionalValue, v => { l.AdditionalValue = v; Notify(); });
	}

	void BuildSpectrumProps(ref int y, SpectrumElement s)
	{
		AddSeparator(ref y, "画像");
		AddSpriteRectSection(ref y, s.Src, v => { s.Src = v; Notify(); });
		AddColorField(ref y, "Color", s.Color, v => { s.Color = v; Notify(); });
		AddColorField(ref y, "WaveColorL", s.WaveColorL, v => { s.WaveColorL = v; Notify(); });
		AddColorField(ref y, "WaveColorR", s.WaveColorR, v => { s.WaveColorR = v; Notify(); });
	}

	void BuildWaveAreaProps(ref int y, WaveAreaElement w)
	{
		AddComboField(ref y, "Target", ["trackbar", "area"], w.Target, v => { w.Target = v; Notify(); });
		AddComboField(ref y, "Mode", ["mix", "stereo", "overlay"], w.Mode, v => { w.Mode = v; Notify(); });
		AddColorField(ref y, "ColorL", w.ColorL, v => { w.ColorL = v; Notify(); });
		AddColorField(ref y, "ColorR", w.ColorR, v => { w.ColorR = v; Notify(); });
		AddColorField(ref y, "ColorMix", w.ColorMix, v => { w.ColorMix = v; Notify(); });
		AddColorField(ref y, "ColorPlayed", w.ColorPlayed, v => { w.ColorPlayed = v; Notify(); });
		AddColorField(ref y, "ColorUnplayed", w.ColorUnplayed, v => { w.ColorUnplayed = v; Notify(); });
	}

	void BuildPictureProps(ref int y, PictureElement p)
	{
		AddSeparator(ref y, "画像");
		AddSpriteRectSection(ref y, p.Src, v => { p.Src = v; Notify(); });
		AddColorField(ref y, "Color", p.Color, v => { p.Color = v; Notify(); });
		AddColorField(ref y, "BorderColor", p.BorderColor, v => { p.BorderColor = v; Notify(); });
		AddIntField(ref y, "BorderWidth", p.BorderWidth, 0, 20, v => { p.BorderWidth = v; Notify(); });
		AddIntField(ref y, "CornerRadius", p.CornerRadius, 0, 999, v => { p.CornerRadius = v; Notify(); });
	}

	void BuildGridProps(ref int y, GridElement g)
	{
		AddColorField(ref y, "BackColor", g.BackColor, v => { g.BackColor = v; Notify(); });
		AddColorField(ref y, "ForeColor", g.ForeColor, v => { g.ForeColor = v; Notify(); });
		AddColorField(ref y, "LineColor", g.LineColor, v => { g.LineColor = v; Notify(); });
		AddColorField(ref y, "HeaderBackColor", g.HeaderBackColor, v => { g.HeaderBackColor = v; Notify(); });
		AddColorField(ref y, "HeaderForeColor", g.HeaderForeColor, v => { g.HeaderForeColor = v; Notify(); });
		AddColorField(ref y, "HeaderLineColor", g.HeaderLineColor, v => { g.HeaderLineColor = v; Notify(); });
	}

	void BuildFormProps(ref int y, FormDef form)
	{
		bool isMainForm = form.FormKey == "MainForm";
		AddHeader(ref y, form.FormKey + " (Form)");
		AddLocationSection(ref y, form.Location, editablePosition: !isMainForm, editableSize: true);

		if (!isMainForm)
		{
			form.Offset ??= new LocationDef();
			AddSeparator(ref y, "メインフォーム基準オフセット");
			AddIntField(ref y, "Offset.X", form.Offset.X, -9999, 9999, v => { form.Offset!.X = v; Notify(); });
			AddIntField(ref y, "Offset.Y", form.Offset.Y, -9999, 9999, v => { form.Offset!.Y = v; Notify(); });
			AddBoolField(ref y, "Magnetic", form.Magnetic, v => { form.Magnetic = v; Notify(); });
			AddColorField(ref y, "BackColor", form.BackColor, v => { form.BackColor = v; Notify(); });
			AddColorField(ref y, "ForeColor", form.ForeColor, v => { form.ForeColor = v; Notify(); });
			AddStringField(ref y, "Font", form.Font, v => { form.Font = v; Notify(); });
			AddIntField(ref y, "FontSize", form.FontSize, 0, 72, v => { form.FontSize = v; Notify(); });
		}

		AddSeparator(ref y, "背景画像");
		AddSpriteRectSection(ref y, form.BackgroundSrc, v => { form.BackgroundSrc = v; Notify(); });
	}

	void Notify() => ElementChanged?.Invoke(this, EventArgs.Empty);

	// ── ヘルパー ──

	void AddHeader(ref int y, string text)
	{
		var lbl = new Label
		{
			Text      = text,
			Left      = 8,
			Top       = y,
			Width     = ClientSize.Width - 16,
			AutoSize  = false,
			Height    = 20,
			ForeColor = Color.FromArgb(160, 200, 255),
			Font      = new Font(Font, FontStyle.Bold),
		};
		Controls.Add(lbl);
		y += 24;
	}

	void AddSeparator(ref int y, string text)
	{
		var lbl = new Label
		{
			Text      = "── " + text,
			Left      = 8,
			Top       = y,
			Width     = ClientSize.Width - 16,
			AutoSize  = false,
			Height    = 18,
			ForeColor = Color.FromArgb(130, 130, 160),
		};
		Controls.Add(lbl);
		y += 22;
	}

	void AddLocationSection(ref int y, LocationDef loc, bool editablePosition = true, bool editableSize = true)
	{
		AddSeparator(ref y, "位置・サイズ");
		if (editablePosition)
		{
			AddIntField(ref y, "X", loc.X, -9999, 9999, v => { loc.X = v; Notify(); });
			AddIntField(ref y, "Y", loc.Y, -9999, 9999, v => { loc.Y = v; Notify(); });
		}
		else
		{
			AddReadOnlyField(ref y, "X", loc.X.ToString());
			AddReadOnlyField(ref y, "Y", loc.Y.ToString());
		}

		if (editableSize)
		{
			AddIntField(ref y, "W", loc.W, 0, 9999, v => { loc.W = v; Notify(); });
			AddIntField(ref y, "H", loc.H, 0, 9999, v => { loc.H = v; Notify(); });
		}
		else
		{
			AddReadOnlyField(ref y, "W", loc.W.ToString());
			AddReadOnlyField(ref y, "H", loc.H.ToString());
		}
	}

	void AddSpriteRectSection(ref int y, SpriteRectDef? rect, Action<SpriteRectDef> onChanged)
	{
		rect ??= new SpriteRectDef();

		var keys = _doc?.Images.Keys.ToArray() ?? [];
		AddComboField(ref y, "ImageKey", keys, rect.ImageKey, v => { rect.ImageKey = v; onChanged(rect); });
		AddIntField(ref y, "Src.X", rect.X, 0, 9999, v => { rect.X = v; onChanged(rect); });
		AddIntField(ref y, "Src.Y", rect.Y, 0, 9999, v => { rect.Y = v; onChanged(rect); });
		AddIntField(ref y, "Src.W", rect.W, 0, 9999, v => { rect.W = v; onChanged(rect); });
		AddIntField(ref y, "Src.H", rect.H, 0, 9999, v => { rect.H = v; onChanged(rect); });

		var pickBtn = new Button
		{
			Text      = "スプライト選択...",
			Left      = 8,
			Top       = y,
			Width     = ClientSize.Width - 16,
			Height    = 22,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(45, 45, 65),
			ForeColor = Color.White,
		};
		pickBtn.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
		pickBtn.Click += (_, __) =>
		{
			if (_doc == null)
				return;
			using var dlg = new Dialogs.SpritePickerDialog(_doc, rect);
			if (dlg.ShowDialog(this) == DialogResult.OK)
			{
				rect.ImageKey = dlg.Result.ImageKey;
				rect.X = dlg.Result.X;
				rect.Y = dlg.Result.Y;
				rect.W = dlg.Result.W;
				rect.H = dlg.Result.H;
				onChanged(rect);
				Rebuild();
			}
		};
		Controls.Add(pickBtn);
		y += 26;
	}

	void AddColorField(ref int y, string label, string? hex, Action<string> onChanged)
	{
		var lbl = MakeLabel(label, y);
		var preview = new Panel
		{
			Left      = 110,
			Top       = y,
			Width     = 20,
			Height    = 18,
			BackColor = ColorHelper.FromHex(hex, Color.Black),
			Cursor    = System.Windows.Forms.Cursors.Hand,
		};
		var box = new TextBox
		{
			Left      = 132,
			Top       = y,
			Width     = ClientSize.Width - 140,
			Text      = hex ?? string.Empty,
			BackColor = Color.FromArgb(35, 35, 50),
			ForeColor = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
		};
		box.TextChanged += (_, __) =>
		{
			preview.BackColor = ColorHelper.FromHex(box.Text, Color.Black);
			onChanged(box.Text.Trim());
		};
		preview.Click += (_, __) =>
		{
			using var dlg = new ColorDialog { Color = preview.BackColor, FullOpen = true };
			if (dlg.ShowDialog(this) != DialogResult.OK)
				return;
			box.Text = ColorHelper.ToHex(dlg.Color);
		};
		Controls.AddRange([lbl, preview, box]);
		y += 24;
	}

	void AddIntField(ref int y, string label, int value, int min, int max, Action<int> onChanged)
	{
		var lbl = MakeLabel(label, y);
		var num = new NumericUpDown
		{
			Left      = 110,
			Top       = y,
			Width     = ClientSize.Width - 118,
			Minimum   = min,
			Maximum   = max,
			Value     = Math.Clamp(value, min, max),
			BackColor = Color.FromArgb(35, 35, 50),
			ForeColor = Color.White,
		};
		num.ValueChanged += (_, __) => onChanged((int)num.Value);
		Controls.AddRange([lbl, num]);
		y += 26;
	}

	void AddStringField(ref int y, string label, string? value, Action<string> onChanged)
	{
		var lbl = MakeLabel(label, y);
		var box = new TextBox
		{
			Left        = 110,
			Top         = y,
			Width       = ClientSize.Width - 118,
			Text        = value ?? string.Empty,
			BackColor   = Color.FromArgb(35, 35, 50),
			ForeColor   = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
		};
		box.TextChanged += (_, __) => onChanged(box.Text);
		Controls.AddRange([lbl, box]);
		y += 26;
	}

	void AddReadOnlyField(ref int y, string label, string value)
	{
		var lbl = MakeLabel(label, y);
		var box = new TextBox
		{
			Left        = 110,
			Top         = y,
			Width       = ClientSize.Width - 118,
			Text        = value,
			ReadOnly    = true,
			BackColor   = Color.FromArgb(28, 28, 40),
			ForeColor   = Color.FromArgb(170, 170, 200),
			BorderStyle = BorderStyle.FixedSingle,
		};
		Controls.AddRange([lbl, box]);
		y += 26;
	}

	void AddBoolField(ref int y, string label, bool value, Action<bool> onChanged)
	{
		var chk = new CheckBox
		{
			Text      = label,
			Left      = 8,
			Top       = y,
			Width     = ClientSize.Width - 16,
			Checked   = value,
			ForeColor = Color.FromArgb(200, 200, 220),
		};
		chk.CheckedChanged += (_, __) => onChanged(chk.Checked);
		Controls.Add(chk);
		y += 24;
	}

	void AddComboField(ref int y, string label, string[] choices, string? current, Action<string> onChanged)
	{
		var lbl = MakeLabel(label, y);
		var combo = new ComboBox
		{
			Left          = 110,
			Top           = y,
			Width         = ClientSize.Width - 118,
			DropDownStyle = ComboBoxStyle.DropDown,
			BackColor     = Color.FromArgb(35, 35, 50),
			ForeColor     = Color.White,
		};
		foreach (var c in choices) combo.Items.Add(c);
		combo.Text = current ?? string.Empty;
		combo.TextChanged  += (_, __) => onChanged(combo.Text);
		combo.SelectedIndexChanged += (_, __) => onChanged(combo.Text);
		Controls.AddRange([lbl, combo]);
		y += 26;
	}

	Label MakeLabel(string text, int y) => new()
	{
		Text      = text,
		Left      = 8,
		Top       = y + 3,
		Width     = 100,
		AutoSize  = false,
		ForeColor = Color.FromArgb(170, 170, 200),
	};
}
