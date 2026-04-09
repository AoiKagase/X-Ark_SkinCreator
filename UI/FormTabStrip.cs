using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Models;

namespace SkinCreator.UI;

class FormTabStrip : Panel
{
	static readonly KnownSubFormDefinition[] KnownSubForms =
	[
		new("PlayListForm", new LocationDef { W = 316, H = 174 }, new LocationDef { X = 0, Y = 0 }, false),
		new("FileInfoForm", new LocationDef { W = 420, H = 520 }, new LocationDef { X = 0, Y = 0 }, false),
		new("MiniPlayerForm", new LocationDef { W = 300, H = 120 }, new LocationDef { X = 0, Y = 0 }, false),
	];

	SkinDocument? _doc;
	string _activeFormKey = "MainForm";
	readonly List<Button> _tabs = [];
	readonly Button _addBtn;

	public event EventHandler<string>? FormKeyChanged;

	public string ActiveFormKey
	{
		get => _activeFormKey;
		set
		{
			_activeFormKey = value;
			UpdateTabStyles();
			FormKeyChanged?.Invoke(this, value);
		}
	}

	public FormTabStrip()
	{
		Height = 28;
		BackColor = Color.FromArgb(30, 30, 40);

		_addBtn = new Button
		{
			Text      = "+",
			Width     = 28,
			Height    = 26,
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.White,
			BackColor = Color.FromArgb(50, 50, 70),
			Font      = new Font("Segoe UI", 12, FontStyle.Bold),
		};
		_addBtn.FlatAppearance.BorderSize = 0;
		_addBtn.Click += AddBtn_Click;
		Controls.Add(_addBtn);
	}

	public void Bind(SkinDocument? doc)
	{
		_doc = doc;
		RebuildTabs();
	}

	void RebuildTabs()
	{
		foreach (var tab in _tabs)
			Controls.Remove(tab);
		_tabs.Clear();

		var keys = new List<string> { "MainForm" };
		if (_doc != null)
			keys.AddRange(_doc.SubForms.Keys);

		int x = 2;
		foreach (var key in keys)
		{
			var tab = CreateTab(key);
			tab.Left = x;
			Controls.Add(tab);
			_tabs.Add(tab);
			x += tab.Width + 2;
		}

		_addBtn.Left = x;
		_addBtn.Top  = 1;

		UpdateTabStyles();
	}

	Button CreateTab(string key)
	{
		var btn = new Button
		{
			Text      = key,
			Width     = TextRenderer.MeasureText(key, Font).Width + 20,
			Height    = 26,
			Top       = 1,
			Tag       = key,
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.White,
			BackColor = Color.FromArgb(40, 40, 55),
		};
		btn.FlatAppearance.BorderSize = 0;
		btn.Click += Tab_Click;
		return btn;
	}

	void Tab_Click(object? sender, EventArgs e)
	{
		if (sender is Button btn && btn.Tag is string key)
			ActiveFormKey = key;
	}

	void AddBtn_Click(object? sender, EventArgs e)
	{
		if (_doc == null)
			return;

		var available = KnownSubForms
			.Where(x => !_doc.SubForms.ContainsKey(x.Key))
			.ToArray();
		using var dlg = new AddFormDialog(available);
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		var key = dlg.FormKey;
		if (string.IsNullOrWhiteSpace(key) || key == "MainForm" || _doc.SubForms.ContainsKey(key))
		{
			MessageBox.Show("フォームキーが無効または重複しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		var definition = available.FirstOrDefault(x => x.Key == key);
		if (definition == null)
			return;

		_doc.SubForms[key] = new Models.FormDef
		{
			FormKey = key,
			Location = definition.Location.Clone(),
			Offset = definition.Offset.Clone(),
			Magnetic = definition.Magnetic,
		};
		_doc.IsDirty = true;
		RebuildTabs();
		ActiveFormKey = key;
	}

	void UpdateTabStyles()
	{
		foreach (var tab in _tabs)
		{
			bool active = tab.Tag as string == _activeFormKey;
			tab.BackColor = active ? Color.FromArgb(60, 60, 90) : Color.FromArgb(40, 40, 55);
			tab.Font      = new Font(Font, active ? FontStyle.Bold : FontStyle.Regular);
		}
	}
}

// フォーム追加ダイアログ（インライン実装）
class AddFormDialog : Form
{
	readonly ComboBox _keyCombo;

	public string FormKey => (_keyCombo.SelectedItem as string) ?? string.Empty;

	public AddFormDialog(IReadOnlyList<KnownSubFormDefinition> availableForms)
	{
		Text            = "サブフォームを追加";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox     = false;
		MinimizeBox     = false;
		StartPosition   = FormStartPosition.CenterParent;
		ClientSize      = new Size(320, 110);
		BackColor       = Color.FromArgb(25, 25, 38);
		ForeColor       = Color.White;

		var lbl = new Label { Text = "フォームキー:", Left = 12, Top = 12, AutoSize = true };
		_keyCombo = new ComboBox
		{
			Left          = 12,
			Top           = 32,
			Width         = 296,
			DropDownStyle = ComboBoxStyle.DropDownList,
			BackColor     = Color.FromArgb(35, 35, 50),
			ForeColor     = Color.White,
		};
		foreach (var form in availableForms)
			_keyCombo.Items.Add(form.Key);
		if (_keyCombo.Items.Count > 0)
			_keyCombo.SelectedIndex = 0;

		var emptyLabel = new Label
		{
			Text      = "追加可能なサブフォームはありません。",
			Left      = 12,
			Top       = 60,
			Width     = 296,
			Height    = 18,
			ForeColor = Color.FromArgb(180, 180, 200),
			Visible   = _keyCombo.Items.Count == 0,
		};

		var ok = new Button
		{
			Text         = "OK",
			Left         = 150,
			Top          = 78,
			Width        = 70,
			DialogResult = DialogResult.OK,
			Enabled      = _keyCombo.Items.Count > 0,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(50, 90, 150),
			ForeColor    = Color.White,
		};
		var cancel = new Button
		{
			Text         = "キャンセル",
			Left         = 228,
			Top          = 78,
			Width        = 80,
			DialogResult = DialogResult.Cancel,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(60, 40, 40),
			ForeColor    = Color.White,
		};

		AcceptButton = ok;
		CancelButton = cancel;
		Controls.AddRange([lbl, _keyCombo, emptyLabel, ok, cancel]);
	}
}

sealed record KnownSubFormDefinition(string Key, LocationDef Location, LocationDef Offset, bool Magnetic);
