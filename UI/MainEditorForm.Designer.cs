using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Models.Elements;

namespace SkinCreator.UI;

public partial class MainEditorForm
{
	FormTabStrip    _tabStrip  = null!;
	ElementTreeView _tree      = null!;
	PropertyPanel   _propPanel = null!;
	CanvasPanel     _canvas    = null!;
	ToolStripButton _undoBtn   = null!, _redoBtn = null!;
	ToolStripDropDownButton _addMenu = null!;
	SplitContainer  _splitMain = null!, _splitLeft = null!;

	void InitializeComponent()
	{
		Text            = "SkinCreator";
		MinimumSize     = new Size(800, 600);
		Size            = new Size(1100, 750);
		StartPosition   = FormStartPosition.CenterScreen;
		BackColor       = Color.FromArgb(20, 20, 30);
		ForeColor       = Color.White;
		KeyPreview      = true;

		// ── メニュー ──
		var menu = new MenuStrip { BackColor = Color.FromArgb(28, 28, 42), ForeColor = Color.White };

		var fileMenu = AddMenu(menu, "ファイル(&F)");
		AddMenuItem(fileMenu, "新規作成(&N)\tCtrl+N", MenuNew_Click);
		AddMenuItem(fileMenu, "開く(&O)...\tCtrl+O", MenuOpen_Click);
		fileMenu.DropDownItems.Add(new ToolStripSeparator());
		AddMenuItem(fileMenu, "上書き保存(&S)\tCtrl+S", MenuSave_Click);
		AddMenuItem(fileMenu, "名前を付けて保存(&A)...\tCtrl+Shift+S", MenuSaveAs_Click);
		fileMenu.DropDownItems.Add(new ToolStripSeparator());
		AddMenuItem(fileMenu, "終了(&X)", MenuExit_Click);

		var skinMenu = AddMenu(menu, "スキン(&K)");
		AddMenuItem(skinMenu, "スキン情報...", MenuMeta_Click);
		AddMenuItem(skinMenu, "画像管理...", MenuImages_Click);

		MainMenuStrip = menu;

		// ── ツールバー ──
		var toolbar = new ToolStrip
		{
			BackColor    = Color.FromArgb(28, 28, 42),
			GripStyle    = ToolStripGripStyle.Hidden,
			RenderMode   = ToolStripRenderMode.System,
			Padding      = new Padding(4, 2, 4, 2),
		};

		toolbar.Items.Add(MakeToolBtn("新規", MenuNew_Click));
		toolbar.Items.Add(MakeToolBtn("開く", MenuOpen_Click));
		toolbar.Items.Add(MakeToolBtn("保存", MenuSave_Click));
		toolbar.Items.Add(new ToolStripSeparator());

		_undoBtn = MakeToolBtn("↩ Undo", UndoBtn_Click);
		_redoBtn = MakeToolBtn("↪ Redo", RedoBtn_Click);
		_undoBtn.Enabled = false;
		_redoBtn.Enabled = false;
		toolbar.Items.Add(_undoBtn);
		toolbar.Items.Add(_redoBtn);
		toolbar.Items.Add(new ToolStripSeparator());

		// 要素追加メニュー
		_addMenu = new ToolStripDropDownButton("+ 追加")
		{
			BackColor = Color.FromArgb(45, 90, 150),
			ForeColor = Color.White,
		};
		foreach (ElementType t in Enum.GetValues<ElementType>())
		{
			var item = new ToolStripMenuItem(t.ToString()) { Tag = t };
			item.Click += AddElementMenu_Click;
			_addMenu.DropDownItems.Add(item);
		}
		toolbar.Items.Add(_addMenu);

		// ── フォームタブ ──
		_tabStrip = new FormTabStrip { Dock = DockStyle.Top };
		_tabStrip.FormKeyChanged += OnFormKeyChanged;

		// ── スプリットコンテナ ──
		_splitMain = new SplitContainer
		{
			Dock      = DockStyle.Fill,
			Orientation = Orientation.Vertical,
			BackColor = Color.FromArgb(15, 15, 25),
		};
		var splitMain = _splitMain;

		// 左ペイン（ツリー + プロパティ）
		_splitLeft = new SplitContainer
		{
			Dock        = DockStyle.Fill,
			Orientation = Orientation.Horizontal,
		};
		var splitLeft = _splitLeft;

		_tree = new ElementTreeView { Dock = DockStyle.Fill };
		_tree.SelectedElementChanged += OnTreeElementChanged;

		_propPanel = new PropertyPanel { Dock = DockStyle.Fill };
		_propPanel.ElementChanged += OnPropertyChanged;

		splitLeft.Panel1.Controls.Add(_tree);
		splitLeft.Panel2.Controls.Add(_propPanel);
		splitMain.Panel1.Controls.Add(splitLeft);

		// 右ペイン（キャンバス + スクロール）
		var scrollPanel = new Panel
		{
			Dock      = DockStyle.Fill,
			AutoScroll = true,
			BackColor = Color.FromArgb(18, 18, 28),
		};

		_canvas = new CanvasPanel { Left = 8, Top = 8 };
		_canvas.SelectedElementChanged += OnCanvasElementChanged;
		scrollPanel.Controls.Add(_canvas);
		splitMain.Panel2.Controls.Add(scrollPanel);

		// ── 組み立て ──
		Controls.Add(splitMain);
		Controls.Add(_tabStrip);
		Controls.Add(toolbar);
		Controls.Add(menu);

		Load += (_, __) =>
		{
			_splitMain.Panel1MinSize    = 200;
			_splitMain.Panel2MinSize    = 300;
			_splitMain.SplitterDistance = 260;
			_splitLeft.SplitterDistance = Math.Max(100, _splitLeft.Height / 2);
		};
	}

	// ── ヘルパー ──

	static ToolStripMenuItem AddMenu(MenuStrip ms, string text)
	{
		var item = new ToolStripMenuItem(text) { BackColor = Color.FromArgb(28, 28, 42), ForeColor = Color.White };
		ms.Items.Add(item);
		return item;
	}

	static void AddMenuItem(ToolStripMenuItem parent, string text, EventHandler handler)
	{
		var item = new ToolStripMenuItem(text) { BackColor = Color.FromArgb(35, 35, 52), ForeColor = Color.White };
		item.Click += handler;
		parent.DropDownItems.Add(item);
	}

	static ToolStripButton MakeToolBtn(string text, EventHandler handler)
	{
		var btn = new ToolStripButton(text)
		{
			BackColor    = Color.FromArgb(40, 40, 58),
			ForeColor    = Color.White,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			Margin       = new Padding(2, 0, 2, 0),
		};
		btn.Click += handler;
		return btn;
	}
}
