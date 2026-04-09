using System.Drawing;
using System.IO.Compression;
using System.Text.Json;
using SkinCreator.Json;
using SkinCreator.Models;

namespace SkinCreator.IO;

static class XskIO
{
	static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented                = true,
		DefaultIgnoreCondition       = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive  = true,
	};

	public static SkinDocument Load(string xskPath)
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "SkinCreator", Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		ZipFile.ExtractToDirectory(xskPath, tempDir, true);

		var jsonFile = FindSkinJson(tempDir)
			?? throw new FileNotFoundException("skin.json が見つかりません。");

		var json = File.ReadAllText(jsonFile);
		var root = JsonSerializer.Deserialize<SkinJsonRoot>(json, JsonOptions)
			?? throw new InvalidDataException("skin.json のパースに失敗しました。");

		var imageDir = Path.GetDirectoryName(jsonFile)!;
		var imageCache = LoadImages(root, imageDir);

		var doc = SkinDocumentJsonBridge.FromJson(root, imageCache);
		doc.CurrentFilePath = xskPath;
		doc.TempDirectory   = tempDir;
		doc.IsDirty         = false;

		// ImageEntry に AbsolutePath をセット
		if (root.Images != null)
		{
			foreach (var kv in root.Images)
			{
				if (doc.Images.TryGetValue(kv.Key, out var entry))
					entry.AbsolutePath = Path.Combine(imageDir, kv.Value.Replace('/', Path.DirectorySeparatorChar));
			}
		}

		return doc;
	}

	public static void Save(SkinDocument doc, string xskPath)
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "SkinCreator_save", Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		try
		{
			// 画像ファイルをコピー
			foreach (var entry in doc.Images.Values)
			{
				var destPath = Path.Combine(tempDir, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

				if (entry.AbsolutePath != null && File.Exists(entry.AbsolutePath))
				{
					File.Copy(entry.AbsolutePath, destPath, true);
				}
				else if (entry.Bitmap != null)
				{
					entry.Bitmap.Save(destPath);
				}
			}

			// skin.json を書き出し
			var root = SkinDocumentJsonBridge.ToJson(doc);
			var jsonStr = JsonSerializer.Serialize(root, JsonOptions);
			File.WriteAllText(Path.Combine(tempDir, "skin.json"), jsonStr, System.Text.Encoding.UTF8);

			// ZIP 作成
			if (File.Exists(xskPath))
				File.Delete(xskPath);
			ZipFile.CreateFromDirectory(tempDir, xskPath);

			doc.CurrentFilePath = xskPath;
			doc.IsDirty = false;
		}
		finally
		{
			try { Directory.Delete(tempDir, true); } catch { }
		}
	}

	public static SkinDocument CreateEmpty() => new()
	{
		Name    = "New Skin",
		IsDirty = false,
	};

	static string? FindSkinJson(string dir)
	{
		var direct = Path.Combine(dir, "skin.json");
		if (File.Exists(direct))
			return direct;
		return Directory.EnumerateFiles(dir, "skin.json", SearchOption.AllDirectories).FirstOrDefault();
	}

	static Dictionary<string, Bitmap> LoadImages(SkinJsonRoot root, string baseDir)
	{
		var cache = new Dictionary<string, Bitmap>();
		if (root.Images == null)
			return cache;

		foreach (var kv in root.Images)
		{
			var path = Path.Combine(baseDir, kv.Value.Replace('/', Path.DirectorySeparatorChar));
			if (!File.Exists(path))
				continue;
			try
			{
				// FileStream 経由でロードしてファイルロックを避ける
				using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				cache[kv.Key] = new Bitmap(fs);
			}
			catch { }
		}

		return cache;
	}
}
