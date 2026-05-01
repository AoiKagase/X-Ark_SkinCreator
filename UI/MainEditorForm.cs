using System.Drawing;
using System.Windows.Forms;
using SkinCreator.IO;
using SkinCreator.Models;
using SkinCreator.Models.Elements;

namespace SkinCreator.UI;

public partial class MainEditorForm : Form
{
	static readonly Dictionary<string, Dictionary<ElementType, string[]>> KnownControlsByForm = new()
	{
		["MainForm"] = new()
		{
			[ElementType.Button] =
			[
				"BtnPlay", "BtnStop", "BtnOpen", "BtnClose", "BtnMinisize", "BtnPlaylist", "BtnSetting",
				"BtnLoop", "BtnRandom", "BtnNext", "BtnSeekForward", "BtnPause", "BtnSeekBack", "BtnBack", "BtnCD"
			],
			[ElementType.Slider] = ["SldVolume", "SldPan", "SldTrack"],
			[ElementType.Label] = ["LabelTitle", "LabelTime"],
			[ElementType.Spectrum] = ["spectrum"],
			[ElementType.WaveArea] = ["waveArea"],
			[ElementType.Picture] = ["picCover"],
		},
		["PlayListForm"] = new()
		{
			[ElementType.Button] = ["PBtnOpen", "PBtnSave", "PBtnRemove", "PBtnUp", "PBtnDown", "PBtnClose", "PBtnClear"],
			[ElementType.Grid] = ["PlayListGrid"],
		},
		["MiniPlayerForm"] = new()
		{
			[ElementType.Button] = ["BtnBack", "BtnPlay", "BtnPause", "BtnStop", "BtnNext", "BtnClose"],
			[ElementType.Slider] = ["SldTrack", "SldVolume"],
			[ElementType.Label] = ["LabelTitle"],
		},
		["FileInfoForm"] = new()
		{
			[ElementType.Label] =
			[
				"lblFileNameVal", "lblFormatVal", "lblBitVal", "lblLengthVal", "lblSampleRateVal", "lblChannelVal",
				"lblTitleKey", "lblArtistKey", "lblAlbumKey", "lblYearKey", "lblTrackKey", "lblFileNameKey",
				"lblFormatKey", "lblChannelKey", "lblSampleRateKey", "lblBitKey", "lblTitleVal", "lblArtistVal",
				"lblAlbumVal", "lblYearVal", "lblTrackVal", "lblLengthKey"
			],
			[ElementType.Picture] = ["picCover"],
		},
	};

	SkinDocument? _doc;

	public MainEditorForm()
	{
		InitializeComponent();
		NewDocument();
	}

	// ── ドキュメント管理 ──

	void NewDocument()
	{
		DisposeCurrentDoc();
		_doc = XskIO.CreateEmpty();
		_doc.IsDirty = false;
		BindDocument();
	}

	void OpenDocument(string path)
	{
		try
		{
			var doc = XskIO.Load(path);
			DisposeCurrentDoc();
			_doc = doc;
			BindDocument();
		}
		catch (Exception ex)
		{
			MessageBox.Show($"スキンの読み込みに失敗しました。\n{ex.Message}", "エラー",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	bool SaveDocument(bool saveAs)
	{
		if (_doc == null)
			return false;

		string? path = _doc.CurrentFilePath;
		if (saveAs || string.IsNullOrEmpty(path))
		{
			using var dlg = new SaveFileDialog
			{
				Title  = "スキンを保存",
				Filter = "NewSkin パッケージ (*.xsk)|*.xsk|すべてのファイル (*.*)|*.*",
			};
			if (!string.IsNullOrEmpty(path))
				dlg.InitialDirectory = System.IO.Path.GetDirectoryName(path);
			if (dlg.ShowDialog(this) != DialogResult.OK)
				return false;
			path = dlg.FileName;
		}

		try
		{
			XskIO.Save(_doc, path);
			UpdateTitle();
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"スキンの保存に失敗しました。\n{ex.Message}", "エラー",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
	}

	void DisposeCurrentDoc()
	{
		_doc?.Dispose();
		_doc = null;
	}

	void BindDocument()
	{
		_tabStrip.Bind(_doc);
		var formKey = _tabStrip.ActiveFormKey;
		_tree.Bind(_doc, formKey);
		_canvas.Bind(_doc, formKey);
		_propPanel.Bind(_doc, _doc?.GetForm(formKey), null);
		UpdateAddMenuState(formKey);
		UpdateTitle();
		UpdateUndoRedoButtons();
	}

	void UpdateTitle()
	{
		string dirty = _doc?.IsDirty == true ? "* " : "";
		string name  = _doc?.Name ?? "無題";
		string path  = _doc?.CurrentFilePath ?? "";
		Text = $"{dirty}{name} — SkinCreator" + (path.Length > 0 ? $" [{path}]" : "");
	}

	void UpdateUndoRedoButtons()
	{
		_undoBtn.Enabled    = _doc?.CanUndo ?? false;
		_redoBtn.Enabled    = _doc?.CanRedo ?? false;
		_undoBtn.ToolTipText = _doc?.UndoDescription ?? string.Empty;
		_redoBtn.ToolTipText = _doc?.RedoDescription ?? string.Empty;
	}

	bool ConfirmDiscard()
	{
		if (_doc?.IsDirty != true)
			return true;

		var res = MessageBox.Show("変更が保存されていません。保存しますか？", "確認",
			MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
		if (res == DialogResult.Yes)
			return SaveDocument(false);
		return res == DialogResult.No;
	}

	// ── メニュー・ツールバーイベント ──

	void MenuNew_Click(object? s, EventArgs e)
	{
		if (!ConfirmDiscard()) return;
		NewDocument();
	}

	void MenuOpen_Click(object? s, EventArgs e)
	{
		if (!ConfirmDiscard()) return;

		using var dlg = new OpenFileDialog
		{
			Title  = "スキンを開く",
			Filter = "NewSkin パッケージ (*.xsk)|*.xsk|すべてのファイル (*.*)|*.*",
		};
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		OpenDocument(dlg.FileName);
	}

	void MenuSave_Click(object? s, EventArgs e) => SaveDocument(false);
	void MenuSaveAs_Click(object? s, EventArgs e) => SaveDocument(true);

	void MenuExit_Click(object? s, EventArgs e)
	{
		if (!ConfirmDiscard()) return;
		Close();
	}

	void MenuMeta_Click(object? s, EventArgs e)
	{
		if (_doc == null) return;
		using var dlg = new Dialogs.MetaEditDialog(_doc);
		if (dlg.ShowDialog(this) == DialogResult.OK)
			UpdateTitle();
	}

	void MenuImages_Click(object? s, EventArgs e)
	{
		if (_doc == null) return;
		using var dlg = new Dialogs.ImageManagerDialog(_doc);
		dlg.ShowDialog(this);
		// 画像が変わったのでキャンバスを再描画
		_canvas.Invalidate();
	}

	void UndoBtn_Click(object? s, EventArgs e)
	{
		_doc?.Undo();
		RefreshAll();
	}

	void RedoBtn_Click(object? s, EventArgs e)
	{
		_doc?.Redo();
		RefreshAll();
	}

	void AddElementMenu_Click(object? s, EventArgs e)
	{
		if (_doc == null || s is not ToolStripMenuItem item || item.Tag is not ElementType type)
			return;

		var formKey = _tabStrip.ActiveFormKey;
		if (!CanAddElement(formKey, type))
		{
			MessageBox.Show("その要素タイプは現在のフォームではサポートされていません。", "情報",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		var key = PromptKnownKey(formKey, type);
		if (key == null) return;

		var form = _doc.GetForm(formKey);
		ISkinElement el = type switch
		{
			ElementType.Button   => new ButtonElement   { ElementKey = key, Location = new() { W = 30, H = 30 } },
			ElementType.Slider   => new SliderElement   { ElementKey = key, Location = new() { W = 100, H = 10 } },
			ElementType.Label    => new LabelElement    { ElementKey = key, Location = new() { W = 100, H = 20 } },
			ElementType.Spectrum => new SpectrumElement { ElementKey = key, Location = new() { W = 100, H = 50 } },
			ElementType.WaveArea => new WaveAreaElement { ElementKey = key, Location = new() { W = 100, H = 20 } },
			ElementType.Picture  => key == "picCover"
				? new PictureElement { ElementKey = key, Location = new() { W = 120, H = 120 }, CornerRadius = 12 }
				: new PictureElement { ElementKey = key, Location = new() { W = 50, H = 50 } },
			ElementType.Grid     => new GridElement     { ElementKey = key, Location = new() { W = 200, H = 150 } },
			_ => throw new InvalidOperationException(),
		};

		_doc.Execute(new Commands.AddElementCommand(form, el));
		RefreshAll();
		_canvas.SetSelectedElement(el);
		_tree.SelectElement(el);
		_propPanel.Bind(_doc, form, el);
	}

	bool CanAddElement(string formKey, ElementType type)
		=> GetAvailableControlKeys(formKey, type).Length > 0;

	void UpdateAddMenuState(string formKey)
	{
		foreach (ToolStripItem item in _addMenu.DropDownItems)
		{
			if (item is ToolStripMenuItem menuItem && menuItem.Tag is ElementType type)
				menuItem.Enabled = CanAddElement(formKey, type);
		}
	}

	string[] GetAvailableControlKeys(string formKey, ElementType type)
	{
		if (!KnownControlsByForm.TryGetValue(formKey, out var formMap) ||
		    !formMap.TryGetValue(type, out var knownKeys))
			return [];

		var form = _doc?.GetForm(formKey);
		if (form == null)
			return [];

		var used = form.AllElements()
			.Where(x => x.Type == type)
			.Select(x => x.ElementKey)
			.ToHashSet(StringComparer.Ordinal);

		return knownKeys.Where(x => !used.Contains(x)).ToArray();
	}

	string? PromptKnownKey(string formKey, ElementType type)
	{
		var candidates = GetAvailableControlKeys(formKey, type);
		if (candidates.Length == 0)
			return null;
		if (candidates.Length == 1)
			return candidates[0];

		using var dlg = new SelectControlDialog(type, candidates);
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return null;
		return dlg.SelectedKey;
	}

	// ── 内部イベント ──

	void OnFormKeyChanged(object? s, string formKey)
	{
		_tree.Bind(_doc, formKey);
		_canvas.SetFormKey(formKey);
		_propPanel.Bind(_doc, _doc?.GetForm(formKey), null);
		UpdateAddMenuState(formKey);
	}

	void OnTreeElementChanged(object? s, ISkinElement? el)
	{
		_canvas.SetSelectedElement(el);
		_propPanel.Bind(_doc, _doc?.GetForm(_tabStrip.ActiveFormKey), el);
	}

	void OnCanvasElementChanged(object? s, ISkinElement? el)
	{
		_tree.SelectElement(el);
		_propPanel.Bind(_doc, _doc?.GetForm(_tabStrip.ActiveFormKey), el);
		UpdateUndoRedoButtons();
		UpdateTitle();
	}

	void OnPropertyChanged(object? s, EventArgs e)
	{
		_canvas.RefreshLayout();
		_doc!.IsDirty = true;
		UpdateTitle();
	}

	void RefreshAll()
	{
		_tree.Refresh_();
		_canvas.Invalidate();
		UpdateUndoRedoButtons();
		UpdateTitle();
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (!ConfirmDiscard())
		{
			e.Cancel = true;
			return;
		}
		base.OnFormClosing(e);
		DisposeCurrentDoc();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Control && e.KeyCode == Keys.Z) { UndoBtn_Click(null, EventArgs.Empty); e.Handled = true; }
		if (e.Control && e.KeyCode == Keys.Y) { RedoBtn_Click(null, EventArgs.Empty); e.Handled = true; }
		if (e.Control && e.KeyCode == Keys.S) { SaveDocument(e.Shift); e.Handled = true; }
	}
}

class SelectControlDialog : Form
{
	readonly ComboBox _combo;

	public string? SelectedKey => _combo.SelectedItem as string;

	public SelectControlDialog(ElementType type, IReadOnlyList<string> candidates)
	{
		Text = $"{type} を追加";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(320, 110);
		BackColor = Color.FromArgb(25, 25, 38);
		ForeColor = Color.White;

		var label = new Label
		{
			Text = "コントロール名:",
			Left = 12,
			Top = 12,
			AutoSize = true,
		};

		_combo = new ComboBox
		{
			Left = 12,
			Top = 32,
			Width = 296,
			DropDownStyle = ComboBoxStyle.DropDownList,
			BackColor = Color.FromArgb(35, 35, 50),
			ForeColor = Color.White,
		};
		foreach (var candidate in candidates)
			_combo.Items.Add(candidate);
		if (_combo.Items.Count > 0)
			_combo.SelectedIndex = 0;

		var ok = new Button
		{
			Text = "OK",
			Left = 150,
			Top = 72,
			Width = 70,
			DialogResult = DialogResult.OK,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(50, 90, 150),
			ForeColor = Color.White,
		};
		var cancel = new Button
		{
			Text = "キャンセル",
			Left = 228,
			Top = 72,
			Width = 80,
			DialogResult = DialogResult.Cancel,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(60, 40, 40),
			ForeColor = Color.White,
		};

		AcceptButton = ok;
		CancelButton = cancel;
		Controls.AddRange([label, _combo, ok, cancel]);
	}
}
