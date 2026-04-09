using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Models;

namespace SkinCreator.UI.Dialogs;

class ImageManagerDialog : Form
{
	readonly SkinDocument _doc;
	readonly ListView _list;

	public ImageManagerDialog(SkinDocument doc)
	{
		_doc = doc;

		Text            = "画像管理";
		FormBorderStyle = FormBorderStyle.Sizable;
		StartPosition   = FormStartPosition.CenterParent;
		Size            = new Size(500, 400);
		BackColor       = Color.FromArgb(25, 25, 38);
		ForeColor       = Color.White;

		_list = new ListView
		{
			Dock          = DockStyle.Fill,
			View          = View.Details,
			FullRowSelect = true,
			BackColor     = Color.FromArgb(30, 30, 45),
			ForeColor     = Color.White,
			BorderStyle   = BorderStyle.None,
		};
		_list.Columns.Add("キー", 150);
		_list.Columns.Add("パス", 300);

		var toolbar = new Panel
		{
			Dock      = DockStyle.Bottom,
			Height    = 40,
			BackColor = Color.FromArgb(20, 20, 32),
		};

		var addBtn = MakeButton("追加", 8);
		var renBtn = MakeButton("名前変更", 88);
		var delBtn = MakeButton("削除", 188);

		addBtn.Click += AddBtn_Click;
		renBtn.Click += RenBtn_Click;
		delBtn.Click += DelBtn_Click;

		toolbar.Controls.AddRange([addBtn, renBtn, delBtn]);
		Controls.AddRange([_list, toolbar]);

		RefreshList();
	}

	Button MakeButton(string text, int x) => new()
	{
		Text      = text,
		Left      = x,
		Top       = 6,
		Width     = 74,
		Height    = 26,
		FlatStyle = FlatStyle.Flat,
		BackColor = Color.FromArgb(45, 45, 65),
		ForeColor = Color.White,
	};

	void RefreshList()
	{
		_list.Items.Clear();
		foreach (var kv in _doc.Images)
		{
			var item = new ListViewItem(kv.Key);
			item.SubItems.Add(kv.Value.RelativePath);
			item.Tag = kv.Key;
			_list.Items.Add(item);
		}
	}

	void AddBtn_Click(object? sender, EventArgs e)
	{
		using var fileDlg = new OpenFileDialog
		{
			Title  = "画像ファイルを選択",
			Filter = "画像ファイル (*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg|すべてのファイル (*.*)|*.*",
		};
		if (fileDlg.ShowDialog(this) != DialogResult.OK)
			return;

		using var keyDlg = new InputDialog("画像キーを入力", Path.GetFileNameWithoutExtension(fileDlg.FileName));
		if (keyDlg.ShowDialog(this) != DialogResult.OK)
			return;

		var key  = keyDlg.Value.Trim();
		var path = fileDlg.FileName;
		if (string.IsNullOrEmpty(key) || _doc.Images.ContainsKey(key))
		{
			MessageBox.Show("キーが無効または重複しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		try
		{
			using var fs  = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			var bmp = new System.Drawing.Bitmap(fs);
			_doc.Images[key] = new ImageEntry
			{
				Key          = key,
				RelativePath = Path.GetFileName(path),
				AbsolutePath = path,
				Bitmap       = bmp,
			};
			_doc.IsDirty = true;
			RefreshList();
		}
		catch (Exception ex)
		{
			MessageBox.Show($"画像の読み込みに失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	void RenBtn_Click(object? sender, EventArgs e)
	{
		if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not string oldKey)
			return;

		using var dlg = new InputDialog("新しいキーを入力", oldKey);
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		var newKey = dlg.Value.Trim();
		if (string.IsNullOrEmpty(newKey) || _doc.Images.ContainsKey(newKey))
		{
			MessageBox.Show("キーが無効または重複しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		var entry = _doc.Images[oldKey];
		entry.Key = newKey;
		_doc.Images.Remove(oldKey);
		_doc.Images[newKey] = entry;
		_doc.IsDirty = true;
		RefreshList();
	}

	void DelBtn_Click(object? sender, EventArgs e)
	{
		if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not string key)
			return;

		var res = MessageBox.Show($"画像キー「{key}」を削除しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		if (res != DialogResult.Yes)
			return;

		if (_doc.Images.TryGetValue(key, out var entry))
			entry.Dispose();
		_doc.Images.Remove(key);
		_doc.IsDirty = true;
		RefreshList();
	}
}

class InputDialog : Form
{
	readonly TextBox _box;
	public string Value => _box.Text;

	public InputDialog(string title, string initial)
	{
		Text            = title;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox     = false;
		MinimizeBox     = false;
		StartPosition   = FormStartPosition.CenterParent;
		ClientSize      = new Size(320, 90);
		BackColor       = Color.FromArgb(25, 25, 38);
		ForeColor       = Color.White;

		_box = new TextBox
		{
			Left        = 12,
			Top         = 12,
			Width       = 296,
			Text        = initial,
			BackColor   = Color.FromArgb(35, 35, 50),
			ForeColor   = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
		};
		var ok = new Button
		{
			Text         = "OK",
			Left         = 150,
			Top          = 50,
			Width        = 70,
			DialogResult = DialogResult.OK,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(50, 90, 150),
			ForeColor    = Color.White,
		};
		var cancel = new Button
		{
			Text         = "キャンセル",
			Left         = 230,
			Top          = 50,
			Width        = 80,
			DialogResult = DialogResult.Cancel,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(60, 40, 40),
			ForeColor    = Color.White,
		};
		AcceptButton = ok;
		CancelButton = cancel;
		Controls.AddRange([_box, ok, cancel]);
		_box.SelectAll();
	}
}
