using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Helpers;
using SkinCreator.Models;
using SkinCreator.Models.Elements;
using GdiCursors = SkinCreator.Helpers.Cursors;

namespace SkinCreator.UI;

class CanvasPanel : Panel
{
	const int CanvasMargin = 16;

	SkinDocument? _doc;
	string _formKey = "MainForm";
	ISkinElement? _selected;
	Point _viewOrigin;

	bool _dragging;
	bool _draggingForm;
	Point _dragStart;
	LocationDef? _dragOrigLoc;
	LocationDef? _dragOrigFormOffset;
	GdiCursors.ResizeDirection _dragMode = GdiCursors.ResizeDirection.None;

	public event EventHandler<ISkinElement?>? SelectedElementChanged;

	public CanvasPanel()
	{
		DoubleBuffered = true;
		BackColor      = Color.FromArgb(18, 18, 28);
	}

	public void Bind(SkinDocument? doc, string formKey)
	{
		_doc      = doc;
		_formKey  = formKey;
		_selected = null;
		UpdateSize();
		Invalidate();
	}

	public void SetFormKey(string formKey)
	{
		_formKey  = formKey;
		_selected = null;
		UpdateSize();
		Invalidate();
	}

	public void SetSelectedElement(ISkinElement? element)
	{
		_selected = element;
		Invalidate();
	}

	public void RefreshLayout()
	{
		UpdateSize();
		Invalidate();
	}

	void UpdateSize()
	{
		if (_doc == null)
		{
			Size = new Size(200, 200);
			_viewOrigin = new Point(CanvasMargin, CanvasMargin);
			return;
		}

		if (_formKey == "MainForm")
		{
			var form = _doc.MainForm;
			int w = Math.Max(form.Location.W, 100) + CanvasMargin * 2;
			int h = Math.Max(form.Location.H, 100) + CanvasMargin * 2;
			_viewOrigin = new Point(CanvasMargin, CanvasMargin);
			Size = new Size(w, h);
			return;
		}

		var subForm = _doc.GetForm(_formKey);
		var offset = subForm.Offset ?? new LocationDef();
		int left = Math.Min(0, offset.X);
		int top = Math.Min(0, offset.Y);
		int right = Math.Max(_doc.MainForm.Location.W, offset.X + Math.Max(subForm.Location.W, 1));
		int bottom = Math.Max(_doc.MainForm.Location.H, offset.Y + Math.Max(subForm.Location.H, 1));
		_viewOrigin = new Point(CanvasMargin - left, CanvasMargin - top);
		Size = new Size(Math.Max(right - left, 100) + CanvasMargin * 2,
			Math.Max(bottom - top, 100) + CanvasMargin * 2);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

		DrawCheckerBackground(g);

		if (_doc == null)
			return;

		var form = _doc.GetForm(_formKey);

		if (_formKey != "MainForm")
			DrawForm(g, _doc.MainForm, GetFormDisplayRectangle(_doc.MainForm));

		DrawForm(g, form, GetFormDisplayRectangle(form));

		foreach (var el in form.AllElements())
			DrawElement(g, el);

		if (_formKey != "MainForm")
		{
			using var pen = new Pen(Color.FromArgb(220, 150, 200, 255), 1);
			g.DrawRectangle(pen, GetFormDisplayRectangle(form));
		}

		// 選択ハンドル
		if (_selected != null)
			HandleRenderer.DrawHandles(g, GetDisplayRect(_selected));
	}

	void DrawForm(Graphics g, FormDef form, Rectangle rect)
	{
		if (form.BackgroundSrc != null)
		{
			using var bg = _doc!.CropBitmap(form.BackgroundSrc);
			if (bg != null)
			{
				g.DrawImage(bg, rect.Location);
				return;
			}
		}

		using var brush = new SolidBrush(Color.FromArgb(30, 55, 55, 75));
		using var pen = new Pen(Color.FromArgb(130, 130, 150, 180), 1);
		g.FillRectangle(brush, rect);
		g.DrawRectangle(pen, rect);
	}

	void DrawCheckerBackground(Graphics g)
	{
		const int cell = 10;
		var c1 = Color.FromArgb(30, 30, 45);
		var c2 = Color.FromArgb(22, 22, 35);
		using var b1 = new SolidBrush(c1);
		using var b2 = new SolidBrush(c2);

		for (int y = 0; y < Height; y += cell)
		{
			for (int x = 0; x < Width; x += cell)
			{
				var brush = ((x / cell + y / cell) % 2 == 0) ? b1 : b2;
				g.FillRectangle(brush, x, y, cell, cell);
			}
		}
	}

	void DrawElement(Graphics g, ISkinElement el)
	{
		var rect = GetDisplayRect(el);
		if (rect.Width == 0 || rect.Height == 0)
			return;

		switch (el)
		{
			case ButtonElement b:
				DrawButton(g, b, rect);
				break;
			case SliderElement s:
				DrawSlider(g, s, rect);
				break;
			case LabelElement l:
				DrawLabel(g, l, rect);
				break;
			case SpectrumElement sp:
				DrawTintedRect(g, rect, Color.FromArgb(80, 0, 200, 80), "Spectrum");
				break;
			case WaveAreaElement:
				DrawTintedRect(g, rect, Color.FromArgb(80, 0, 150, 200), "Wave");
				break;
			case PictureElement p:
				DrawPicture(g, p, rect);
				break;
			case GridElement:
				DrawTintedRect(g, rect, Color.FromArgb(80, 100, 50, 200), "Grid");
				break;
		}

		if (el.IsDisabled)
		{
			using var hatch = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal,
				Color.FromArgb(90, Color.Black), Color.FromArgb(30, Color.White));
			g.FillRectangle(hatch, rect);
		}
	}

	void DrawButton(Graphics g, ButtonElement b, Rectangle rect)
	{
		using var img = _doc!.CropBitmap(b.Up);
		if (img != null)
		{
			g.DrawImage(img, rect.Location);
			return;
		}
		DrawTintedRect(g, rect, Color.FromArgb(80, 200, 100, 0), b.ElementKey);
	}

	void DrawSlider(Graphics g, SliderElement s, Rectangle rect)
	{
		using var img = _doc!.CropBitmap(s.Src);
		if (img != null)
		{
			g.DrawImage(img, rect.Location);
			return;
		}
		DrawTintedRect(g, rect, Color.FromArgb(80, 0, 100, 200), s.ElementKey);
	}

	void DrawLabel(Graphics g, LabelElement l, Rectangle rect)
	{
		using var back = new SolidBrush(ColorHelper.FromHex(l.BackColor));
		g.FillRectangle(back, rect);

		var style = FontStyle.Regular;
		if (l.Bold)   style |= FontStyle.Bold;
		if (l.Italic) style |= FontStyle.Italic;

		using var font = new Font(l.Font, Math.Max(l.Size, 6), style, GraphicsUnit.Point);
		using var fore = new SolidBrush(ColorHelper.FromHex(l.ForeColor, Color.White));
		var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
		g.DrawString(l.ElementKey, font, fore, rect, sf);
	}

	void DrawPicture(Graphics g, PictureElement p, Rectangle rect)
	{
		using var img = _doc!.CropBitmap(p.Src);
		if (img != null)
		{
			g.DrawImage(img, rect.Location);
		}
		else
		{
			using var back = new SolidBrush(ColorHelper.FromHex(p.Color));
			g.FillRectangle(back, rect);
		}

		if (p.BorderWidth > 0 && p.BorderColor != null)
		{
			using var pen = new Pen(ColorHelper.FromHex(p.BorderColor), p.BorderWidth);
			g.DrawRectangle(pen, rect);
		}
	}

	void DrawTintedRect(Graphics g, Rectangle rect, Color tint, string label)
	{
		using var brush = new SolidBrush(tint);
		g.FillRectangle(brush, rect);
		using var pen = new Pen(Color.FromArgb(180, tint.R, tint.G, tint.B));
		g.DrawRectangle(pen, rect);

		if (rect.Width > 20 && rect.Height > 12)
		{
			using var fore = new SolidBrush(Color.White);
			var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
			g.DrawString(label, Font, fore, rect, sf);
		}
	}

	// ── マウス操作 ──

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (_doc == null || e.Button != MouseButtons.Left)
			return;

		_dragStart = e.Location;

		// ハンドル当たり判定
		if (_selected != null)
		{
			_dragMode = HandleRenderer.HitTest(GetDisplayRect(_selected), e.Location);
			if (_dragMode != GdiCursors.ResizeDirection.None)
			{
				_dragOrigLoc = _selected.Location.Clone();
				_dragging = true;
				return;
			}
		}

		// 要素当たり判定（後ろから）
		var form = _doc.GetForm(_formKey);
		ISkinElement? hit = null;
		var elements = form.AllElements().ToList();
		for (int i = elements.Count - 1; i >= 0; i--)
		{
			if (GetDisplayRect(elements[i]).Contains(e.Location))
			{
				hit = elements[i];
				break;
			}
		}

		_selected = hit;
		SelectedElementChanged?.Invoke(this, _selected);

		if (_selected != null)
		{
			_dragMode    = GdiCursors.ResizeDirection.Move;
			_dragOrigLoc = _selected.Location.Clone();
			_dragging    = true;
		}
		else
		{
			if (_formKey != "MainForm" && GetFormDisplayRectangle(form).Contains(e.Location))
			{
				form.Offset ??= new LocationDef();
				_dragOrigFormOffset = form.Offset.Clone();
				_draggingForm = true;
			}
			_dragging = false;
		}

		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (_doc == null)
			return;

		// カーソル変更
		if (!_dragging && _selected != null)
		{
			var dir = HandleRenderer.HitTest(GetDisplayRect(_selected), e.Location);
			Cursor = GdiCursors.GetCursor(dir);
		}

		if (_draggingForm)
		{
			var form = _doc.GetForm(_formKey);
			form.Offset ??= new LocationDef();
			form.Offset.X = (_dragOrigFormOffset?.X ?? 0) + (e.X - _dragStart.X);
			form.Offset.Y = (_dragOrigFormOffset?.Y ?? 0) + (e.Y - _dragStart.Y);
			UpdateSize();
			Invalidate();
			SelectedElementChanged?.Invoke(this, null);
			return;
		}

		if (!_dragging || _selected == null || _dragOrigLoc == null)
			return;

		int dx = e.X - _dragStart.X;
		int dy = e.Y - _dragStart.Y;
		var loc = _selected.Location;

		switch (_dragMode)
		{
			case GdiCursors.ResizeDirection.Move:
				loc.X = _dragOrigLoc.X + dx;
				loc.Y = _dragOrigLoc.Y + dy;
				break;
			case GdiCursors.ResizeDirection.SE:
				loc.W = Math.Max(4, _dragOrigLoc.W + dx);
				loc.H = Math.Max(4, _dragOrigLoc.H + dy);
				break;
			case GdiCursors.ResizeDirection.E:
				loc.W = Math.Max(4, _dragOrigLoc.W + dx);
				break;
			case GdiCursors.ResizeDirection.S:
				loc.H = Math.Max(4, _dragOrigLoc.H + dy);
				break;
			case GdiCursors.ResizeDirection.NW:
				loc.X = _dragOrigLoc.X + dx;
				loc.Y = _dragOrigLoc.Y + dy;
				loc.W = Math.Max(4, _dragOrigLoc.W - dx);
				loc.H = Math.Max(4, _dragOrigLoc.H - dy);
				break;
			case GdiCursors.ResizeDirection.N:
				loc.Y = _dragOrigLoc.Y + dy;
				loc.H = Math.Max(4, _dragOrigLoc.H - dy);
				break;
			case GdiCursors.ResizeDirection.SW:
				loc.X = _dragOrigLoc.X + dx;
				loc.W = Math.Max(4, _dragOrigLoc.W - dx);
				loc.H = Math.Max(4, _dragOrigLoc.H + dy);
				break;
			case GdiCursors.ResizeDirection.NE:
				loc.Y = _dragOrigLoc.Y + dy;
				loc.W = Math.Max(4, _dragOrigLoc.W + dx);
				loc.H = Math.Max(4, _dragOrigLoc.H - dy);
				break;
		}

		Invalidate();
		SelectedElementChanged?.Invoke(this, _selected);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (!_dragging || _selected == null || _dragOrigLoc == null || _doc == null)
		{
			if (_draggingForm && _doc != null)
			{
				_doc.IsDirty = true;
				_draggingForm = false;
				Cursor = System.Windows.Forms.Cursors.Default;
				return;
			}
			_dragging = false;
			return;
		}

		var newLoc = _selected.Location.Clone();
		// 実際に移動していない場合はコマンド不要
		if (newLoc.X != _dragOrigLoc.X || newLoc.Y != _dragOrigLoc.Y ||
		    newLoc.W != _dragOrigLoc.W || newLoc.H != _dragOrigLoc.H)
		{
			var cmd = new Commands.MoveElementCommand(_selected, _dragOrigLoc, newLoc);
			// Execute はすでに実行済みなので Undo スタックだけに積む
			_doc.IsDirty = true;
		}

		_dragging = false;
		_draggingForm = false;
		Cursor = System.Windows.Forms.Cursors.Default;
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right)
			return true;
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (_selected == null || _doc == null)
			return;

		int step = e.Shift ? 10 : 1;
		var loc  = _selected.Location;

		switch (e.KeyCode)
		{
			case Keys.Left:  loc.X -= step; break;
			case Keys.Right: loc.X += step; break;
			case Keys.Up:    loc.Y -= step; break;
			case Keys.Down:  loc.Y += step; break;
			case Keys.Delete:
				var form = _doc.GetForm(_formKey);
				_doc.Execute(new Commands.RemoveElementCommand(form, _selected));
				_selected = null;
				SelectedElementChanged?.Invoke(this, null);
				break;
		}

		_doc.IsDirty = true;
		Invalidate();
		SelectedElementChanged?.Invoke(this, _selected);
		e.Handled = true;
	}

	Rectangle GetDisplayRect(ISkinElement el)
	{
		var rect = GetLogicalRect(el);
		if (_formKey != "MainForm")
		{
			var formRect = GetFormDisplayRectangle(_doc!.GetForm(_formKey));
			rect.Offset(formRect.Left, formRect.Top);
		}
		else
		{
			rect.Offset(_viewOrigin);
		}
		return rect;
	}

	Rectangle GetLogicalRect(ISkinElement el)
	{
		var rect = el.Location.ToRectangle();
		if (el is ButtonElement button)
		{
			var size = GetButtonVisualSize(button);
			rect.Width = size.Width;
			rect.Height = size.Height;
		}
		return rect;
	}

	Size GetButtonVisualSize(ButtonElement button)
	{
		var states = new[] { button.Up, button.Down, button.Hover, button.Disabled, button.Optional };
		foreach (var state in states)
		{
			if (state == null)
				continue;

			if (state.W > 0 && state.H > 0)
				return new Size(state.W, state.H);

			using var bmp = _doc!.CropBitmap(state);
			if (bmp != null)
				return bmp.Size;
		}

		if (button.Location.W > 0 && button.Location.H > 0)
			return new Size(button.Location.W, button.Location.H);

		return Size.Empty;
	}

	Rectangle GetFormDisplayRectangle(FormDef form)
	{
		var rect = new Rectangle(0, 0, Math.Max(form.Location.W, 1), Math.Max(form.Location.H, 1));
		if (form.FormKey == "MainForm")
		{
			rect.Offset(_viewOrigin);
			return rect;
		}

		var offset = form.Offset ?? new LocationDef();
		rect.Offset(_viewOrigin.X + offset.X, _viewOrigin.Y + offset.Y);
		return rect;
	}
}
