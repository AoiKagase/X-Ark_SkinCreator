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
		_canvas = new SpriteCanvas { Dock = DockStyle.Fill };
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
		_canvas.Size   = bmp != null ? bmp.Size : new Size(100, 100);
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
		Bitmap? _bitmap;
		Rectangle _selection;
		bool _drawing;
		Point _drawStart;

		public event Action? SelectionChanged;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Bitmap? Bitmap
		{
			get => _bitmap;
			set { _bitmap = value; Invalidate(); }
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
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.Clear(Color.FromArgb(18, 18, 28));

			if (_bitmap != null)
				g.DrawImage(_bitmap, Point.Empty);

			if (_selection.Width > 0 && _selection.Height > 0)
			{
				using var pen = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
				g.DrawRectangle(pen, _selection);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			_drawing   = true;
			_drawStart = e.Location;
			_selection = Rectangle.Empty;
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_drawing) return;
			int x = Math.Min(e.X, _drawStart.X);
			int y = Math.Min(e.Y, _drawStart.Y);
			int w = Math.Abs(e.X - _drawStart.X);
			int h = Math.Abs(e.Y - _drawStart.Y);
			_selection = new Rectangle(x, y, w, h);
			Invalidate();
			SelectionChanged?.Invoke();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_drawing = false;
			SelectionChanged?.Invoke();
		}
	}
}
