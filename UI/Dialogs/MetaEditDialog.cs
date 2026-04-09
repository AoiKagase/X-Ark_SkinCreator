using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Models;

namespace SkinCreator.UI.Dialogs;

class MetaEditDialog : Form
{
	readonly TextBox _nameBox, _authorBox, _descBox, _keyBox;
	readonly SkinDocument _doc;

	public MetaEditDialog(SkinDocument doc)
	{
		_doc = doc;

		Text            = "スキン情報";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox     = false;
		MinimizeBox     = false;
		StartPosition   = FormStartPosition.CenterParent;
		ClientSize      = new Size(360, 220);
		BackColor       = Color.FromArgb(25, 25, 38);
		ForeColor       = Color.White;

		int y = 12;
		_nameBox   = AddField("スキン名:",   doc.Name,           ref y);
		_authorBox = AddField("作者:",        doc.Author,         ref y);
		_descBox   = AddField("説明:",        doc.Description,    ref y);
		_keyBox    = AddField("透明キー色:",  doc.TransparentKey, ref y);

		var ok = new Button
		{
			Text         = "OK",
			Left         = 180,
			Top          = y + 8,
			Width        = 80,
			DialogResult = DialogResult.OK,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(50, 90, 150),
			ForeColor    = Color.White,
		};
		var cancel = new Button
		{
			Text         = "キャンセル",
			Left         = 270,
			Top          = y + 8,
			Width        = 80,
			DialogResult = DialogResult.Cancel,
			FlatStyle    = FlatStyle.Flat,
			BackColor    = Color.FromArgb(60, 40, 40),
			ForeColor    = Color.White,
		};
		ok.Click += Ok_Click;
		AcceptButton = ok;
		CancelButton = cancel;
		Controls.AddRange([ok, cancel]);
	}

	TextBox AddField(string label, string value, ref int y)
	{
		var lbl = new Label { Text = label, Left = 12, Top = y + 3, Width = 90, AutoSize = false, ForeColor = Color.White };
		var box = new TextBox
		{
			Left        = 110,
			Top         = y,
			Width       = 238,
			Text        = value,
			BackColor   = Color.FromArgb(35, 35, 50),
			ForeColor   = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
		};
		Controls.AddRange([lbl, box]);
		y += 30;
		return box;
	}

	void Ok_Click(object? sender, EventArgs e)
	{
		_doc.Name           = _nameBox.Text.Trim();
		_doc.Author         = _authorBox.Text.Trim();
		_doc.Description    = _descBox.Text.Trim();
		_doc.TransparentKey = _keyBox.Text.Trim();
		_doc.IsDirty        = true;
	}
}
