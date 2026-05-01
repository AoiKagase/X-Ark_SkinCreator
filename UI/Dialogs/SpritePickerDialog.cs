using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Helpers;
using SkinCreator.Models;

namespace SkinCreator.UI.Dialogs;

class SpritePickerDialog : Form
{
	readonly SkinDocument _doc;
	readonly ComboBox _keyCombo;
	readonly Panel _previewContainer;
	readonly SpriteCanvas _canvas;
	readonly NumericUpDown _numX, _numY, _numW, _numH;
	bool _updating;

	public SpriteRectDef Result { get; private set; } = new();

	public SpritePickerDialog(SkinDocument doc, SpriteRectDef? initial)
	{
		_doc = doc;

		Text            = "スプライト選択";
		FormBorderStyle = FormBorderStyle.Sizable;
		StartPosition   = FormStartPosition.CenterParent;
		MinimumSize     = new Size(500, 500);
		Size            = new Size(700, 600);
		BackColor       = Color.FromArgb(25, 25, 38);
		ForeColor       = Color.White;

		// 画像キー選択
		var keyLabel = new Label { Text = "画像キー:", Left = 8, Top = 12, AutoSize = true };
		_keyCombo = new ComboBox
		{
			Left          = 80,
			Top           = 8,
			Width         = 300,
			DropDownStyle = ComboBoxStyle.DropDownList,
			BackColor     = Color.FromArgb(35, 35, 50),
			ForeColor     = Color.White,
		};
		foreach (var key in doc.Images.Keys) _keyCombo.Items.Add(key);

		// プレビュー
		_canvas = new SpriteCanvas { Left = 0, Top = 0 };
		_previewContainer = new Panel
		{
			Left        = 8,
			Top         = 38,
			Width       = ClientSize.Width - 16,
			Height      = ClientSize.Height - 140,
			Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
			BackColor   = Color.FromArgb(18, 18, 28),
			AutoScroll  = true,
		};
		_previewContainer.Controls.Add(_canvas);
		_previewContainer.MouseWheel += (_, e) => _canvas.HandleContainerMouseWheel(e);

		// 矩形入力
		int numTop = ClientSize.Height - 95;
		_numX = MakeNum("X:", 8,   numTop);
		_numY = MakeNum("Y:", 108, numTop);
		_numW = MakeNum("W:", 208, numTop);
		_numH = MakeNum("H:", 308, numTop);

		foreach (var n in new[] { _numX, _numY, _numW, _numH })
			n.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

		// OK / Cancel
		var ok = new Button
		{
			Text         = "OK",
			DialogResult = DialogResult.OK,
			Left         = ClientSize.Width - 180,
			Top          = ClientSize.Height - 50,
			Width        = 80,
			Anchor       = AnchorStyles.Right | AnchorStyles.Bottom,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(50, 90, 150),
			ForeColor    = Color.White,
		};
		var cancel = new Button
		{
			Text         = "キャンセル",
			DialogResult = DialogResult.Cancel,
			Left         = ClientSize.Width - 90,
			Top          = ClientSize.Height - 50,
			Width        = 82,
			Anchor       = AnchorStyles.Right | AnchorStyles.Bottom,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(60, 40, 40),
			ForeColor    = Color.White,
		};

		AcceptButton = ok;
		CancelButton = cancel;

		Controls.AddRange([keyLabel, _keyCombo, _previewContainer, ok, cancel]);
		// numericupdowns を別途追加（foreachでは型推論が難しいため）
		Controls.Add(_numX.Parent!);  // ← GroupBox ではなく直接 Controls に
		foreach (var ctrl in new Control[] { _numX, _numY, _numW, _numH })
		{
			ctrl.Parent?.Controls.Remove(ctrl);
			Controls.Add(ctrl);
		}

		// イベント
		_keyCombo.SelectedIndexChanged += (_, __) => OnKeyChanged();
		_canvas.SelectionChanged += OnCanvasSelection;
		foreach (var n in new[] { _numX, _numY, _numW, _numH })
			n.ValueChanged += (_, __) => OnNumChanged();

		ok.Click += (_, __) => BuildResult();

		// 初期値設定
		if (initial != null && !string.IsNullOrEmpty(initial.ImageKey))
		{
			_keyCombo.SelectedItem = initial.ImageKey;
			_numX.Value = initial.X;
			_numY.Value = initial.Y;
			_numW.Value = initial.W;
			_numH.Value = initial.H;
			_canvas.Selection = new Rectangle(initial.X, initial.Y, initial.W, initial.H);
		}
		else if (_keyCombo.Items.Count > 0)
		{
			_keyCombo.SelectedIndex = 0;
		}

		Resize += (_, __) => RepositionNumericBoxes();
	}

	NumericUpDown MakeNum(string label, int x, int top)
	{
		var lbl = new Label { Text = label, Left = x, Top = top + 3, AutoSize = true, ForeColor = Color.White };
		Controls.Add(lbl);
		var num = new NumericUpDown
		{
			Left      = x + 20,
			Top       = top,
			Width     = 68,
			Minimum   = 0,
			Maximum   = 9999,
			BackColor = Color.FromArgb(35, 35, 50),
			ForeColor = Color.White,
		};
		return num;
	}

	void RepositionNumericBoxes()
	{
		int top = ClientSize.Height - 95;
		_numX.Top = top; _numY.Top = top; _numW.Top = top; _numH.Top = top;
	}

	void OnKeyChanged()
	{
		if (_keyCombo.SelectedItem is not string key) return;
		var bmp = _doc.GetBitmap(key);
		_canvas.Bitmap = bmp;
	}

	void OnCanvasSelection()
	{
		_updating = true;
		var sel = _canvas.Selection;
		_numX.Value = sel.X;
		_numY.Value = sel.Y;
		_numW.Value = sel.Width;
		_numH.Value = sel.Height;
		_updating = false;
	}

	void OnNumChanged()
	{
		if (_updating) return;
		_canvas.Selection = new Rectangle((int)_numX.Value, (int)_numY.Value, (int)_numW.Value, (int)_numH.Value);
		_canvas.Invalidate();
	}

	void BuildResult()
	{
		Result = new SpriteRectDef
		{
			ImageKey = _keyCombo.SelectedItem as string,
			X = (int)_numX.Value,
			Y = (int)_numY.Value,
			W = (int)_numW.Value,
			H = (int)_numH.Value,
		};
	}

	// 画像表示 + ゴムバンド矩形選択キャンバス
	class SpriteCanvas : Control
	{
		const float MinZoom = 0.25f;
		const float MaxZoom = 8.0f;

		Bitmap? _bitmap;
		Rectangle _selection;
		bool _drawing;
		Point _drawStart;
		float _zoom = 1.0f;

		public event Action? SelectionChanged;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Bitmap? Bitmap
		{
			get => _bitmap;
			set
			{
				_bitmap = value;
				UpdateCanvasSize();
				Invalidate();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle Selection
		{
			get => _selection;
			set { _selection = value; Invalidate(); }
		}

		public SpriteCanvas()
		{
			DoubleBuffered = true;
			Cursor         = System.Windows.Forms.Cursors.Cross;
			TabStop        = true;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.Clear(Color.FromArgb(18, 18, 28));
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

			if (_bitmap != null)
				g.DrawImage(_bitmap, new Rectangle(0, 0, ScaleLength(_bitmap.Width), ScaleLength(_bitmap.Height)));

			if (_selection.Width > 0 && _selection.Height > 0)
			{
				using var pen = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
				g.DrawRectangle(pen, ScaleRect(_selection));
			}

			DrawZoomIndicator(g);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Focus();
			_drawing   = true;
			_drawStart = ToImagePoint(e.Location);
			_selection = Rectangle.Empty;
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_drawing) return;
			var imagePoint = ToImagePoint(e.Location);
			int x = Math.Min(imagePoint.X, _drawStart.X);
			int y = Math.Min(imagePoint.Y, _drawStart.Y);
			int w = Math.Abs(imagePoint.X - _drawStart.X);
			int h = Math.Abs(imagePoint.Y - _drawStart.Y);
			_selection = new Rectangle(x, y, w, h);
			Invalidate();
			SelectionChanged?.Invoke();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_drawing = false;
			SelectionChanged?.Invoke();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Focus();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if ((ModifierKeys & Keys.Control) != Keys.Control)
			{
				base.OnMouseWheel(e);
				return;
			}

			ZoomAt(e.Location, e.Delta > 0 ? 1 : -1);
		}

		public void HandleContainerMouseWheel(MouseEventArgs e)
		{
			if ((ModifierKeys & Keys.Control) != Keys.Control)
				return;

			ZoomAt(PointToClient(Parent!.PointToScreen(e.Location)), e.Delta > 0 ? 1 : -1);
		}

		void ZoomAt(Point mouseLocation, int direction)
		{
			var oldZoom = _zoom;
			_zoom = Math.Clamp(_zoom * (direction > 0 ? 1.1f : 1 / 1.1f), MinZoom, MaxZoom);
			if (Math.Abs(_zoom - oldZoom) < 0.001f)
				return;

			UpdateCanvasSize();
			Invalidate();
			KeepMouseAnchorAfterZoom(mouseLocation, oldZoom);
		}

		void UpdateCanvasSize()
		{
			var sourceSize = _bitmap?.Size ?? new Size(100, 100);
			Size = new Size(ScaleLength(sourceSize.Width), ScaleLength(sourceSize.Height));
		}

		Point ToImagePoint(Point displayPoint) => new(
			Math.Max(0, (int)Math.Floor(displayPoint.X / _zoom)),
			Math.Max(0, (int)Math.Floor(displayPoint.Y / _zoom)));

		Rectangle ScaleRect(Rectangle rect) => new(
			ScaleCoordinate(rect.X),
			ScaleCoordinate(rect.Y),
			ScaleLength(rect.Width),
			ScaleLength(rect.Height));

		int ScaleCoordinate(int value) => (int)Math.Round(value * _zoom, MidpointRounding.AwayFromZero);

		int ScaleLength(int value) => Math.Max(1, (int)Math.Round(value * _zoom, MidpointRounding.AwayFromZero));

		void KeepMouseAnchorAfterZoom(Point mouseLocation, float oldZoom)
		{
			if (Parent is not ScrollableControl scroll)
				return;

			var oldScroll = new Point(-scroll.AutoScrollPosition.X, -scroll.AutoScrollPosition.Y);
			var logicalX = (oldScroll.X + mouseLocation.X) / oldZoom;
			var logicalY = (oldScroll.Y + mouseLocation.Y) / oldZoom;
			scroll.AutoScrollPosition = new Point(
				Math.Max(0, (int)Math.Round(logicalX * _zoom - mouseLocation.X)),
				Math.Max(0, (int)Math.Round(logicalY * _zoom - mouseLocation.Y)));
		}

		void DrawZoomIndicator(Graphics g)
		{
			var text = $"{_zoom * 100:0}%";
			using var font = new Font(Font.FontFamily, 9, FontStyle.Regular, GraphicsUnit.Point);
			var size = g.MeasureString(text, font);
			var rect = new RectangleF(
				Width - size.Width - 18,
				8,
				size.Width + 10,
				size.Height + 4);
			using var back = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
			using var fore = new SolidBrush(Color.White);
			g.FillRectangle(back, rect);
			g.DrawString(text, font, fore, rect.Left + 5, rect.Top + 2);
		}
	}
}
