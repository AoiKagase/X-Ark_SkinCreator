using System.Drawing;
using System.Windows.Forms;
using SkinCreator.Models;
using SkinCreator.Models.Elements;

namespace SkinCreator.UI;

class ElementTreeView : TreeView
{
	SkinDocument? _doc;
	string _formKey = "MainForm";

	public event EventHandler<ISkinElement?>? SelectedElementChanged;

	public ElementTreeView()
	{
		BackColor    = Color.FromArgb(22, 22, 32);
		ForeColor    = Color.FromArgb(210, 210, 225);
		BorderStyle  = BorderStyle.None;
		HideSelection = false;
		AfterSelect  += OnAfterSelect;

		BuildContextMenu();
	}

	public void Bind(SkinDocument? doc, string formKey)
	{
		_doc     = doc;
		_formKey = formKey;
		Refresh_();
	}

	public void Refresh_(string? formKey = null)
	{
		if (formKey != null) _formKey = formKey;

		BeginUpdate();
		Nodes.Clear();

		if (_doc == null)
		{
			EndUpdate();
			return;
		}

		var form = _doc.GetForm(_formKey);
		AddCategoryNode("ボタン", form.Buttons.Cast<ISkinElement>());
		AddCategoryNode("スライダー", form.Sliders.Cast<ISkinElement>());
		AddCategoryNode("ラベル", form.Labels.Cast<ISkinElement>());
		if (form.Spectrum != null) AddLeafNode("スペクトラム", form.Spectrum);
		if (form.WaveArea != null) AddLeafNode("波形エリア", form.WaveArea);
		AddCategoryNode("ピクチャ", form.Pictures.Cast<ISkinElement>());
		AddCategoryNode("グリッド", form.Grids.Cast<ISkinElement>());

		ExpandAll();
		EndUpdate();
	}

	public void SelectElement(ISkinElement? element)
	{
		if (element == null)
		{
			SelectedNode = null;
			return;
		}

		foreach (TreeNode cat in Nodes)
		{
			foreach (TreeNode child in cat.Nodes)
			{
				if (child.Tag == element)
				{
					SelectedNode = child;
					return;
				}
			}
			if (cat.Tag == element)
			{
				SelectedNode = cat;
				return;
			}
		}
	}

	void AddCategoryNode(string label, IEnumerable<ISkinElement> elements)
	{
		var list = elements.ToList();
		if (list.Count == 0)
			return;

		var cat = new TreeNode(label) { ForeColor = Color.FromArgb(140, 140, 180) };
		foreach (var el in list)
		{
			var node = new TreeNode(el.ElementKey) { Tag = el, ForeColor = Color.FromArgb(210, 210, 225) };
			cat.Nodes.Add(node);
		}
		Nodes.Add(cat);
	}

	void AddLeafNode(string label, ISkinElement element)
	{
		var node = new TreeNode(label) { Tag = element };
		Nodes.Add(node);
	}

	void OnAfterSelect(object? sender, TreeViewEventArgs e)
	{
		var element = e.Node?.Tag as ISkinElement;
		SelectedElementChanged?.Invoke(this, element);
	}

	void BuildContextMenu()
	{
		var menu = new ContextMenuStrip();
		var deleteItem = new ToolStripMenuItem("削除");
		deleteItem.Click += DeleteItem_Click;
		menu.Items.Add(deleteItem);
		ContextMenuStrip = menu;
	}

	void DeleteItem_Click(object? sender, EventArgs e)
	{
		if (_doc == null || SelectedNode?.Tag is not ISkinElement element)
			return;

		var form = _doc.GetForm(_formKey);
		var cmd  = new Commands.RemoveElementCommand(form, element);
		_doc.Execute(cmd);
		Refresh_();
		SelectedElementChanged?.Invoke(this, null);
	}
}
