using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class KielRegionCsvExporter
{
    private const string CsvFileName = "projects.csv";
    private const string ImagesFolderName = "KielRegionImages";

    [MenuItem("KielRegion/Export CSV from ScriptableObjects")]
    public static void Export()
    {
        // Find all ScriptableObjects of type KielRegionProjectDataObject in the project
        var guids = AssetDatabase.FindAssets("t:KielRegionProjectDataObject");
        var assets = new List<KielRegionProjectDataObject>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<KielRegionProjectDataObject>(path);
            if (asset != null)
                assets.Add(asset);
        }

        if (assets.Count == 0)
        {
            Debug.LogWarning("No KielRegionProjectDataObject assets found.");
            return;
        }

        // Ensure StreamingAssets exists
        var streamingAssetsPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamingAssetsPath)) Directory.CreateDirectory(streamingAssetsPath);

        // Ensure images directory exists
        var imagesDir = Path.Combine(streamingAssetsPath, ImagesFolderName);
        if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

        // Build CSV
        var csvPath = Path.Combine(streamingAssetsPath, CsvFileName);
        using (var writer = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8))
        {
            writer.WriteLine("title;category;imageFiles;shortDescription;additionalInfo");
            foreach (var a in assets.OrderBy(x => x.id))
            {
                // Copy images to StreamingAssets and collect file names
                var imageFileNames = new List<string>();
                if (a.projectImage != null)
                {
                    for (int idx = 0; idx < a.projectImage.Count; idx++)
                    {
                        var sprite = a.projectImage[idx];
                        if (sprite == null || sprite.texture == null) continue;
                        var baseName = SanitizeForFileName(a.title);
                        var fileName = $"{baseName}_{idx + 1}.png";
                        var dstPath = Path.Combine(imagesDir, fileName);

                        // Reimport texture as readable and encode to PNG bytes
                        try
                        {
                            var readableTex = GetReadableTexture(sprite.texture);
                            var pngBytes = readableTex.EncodeToPNG();
                            File.WriteAllBytes(dstPath, pngBytes);
                            imageFileNames.Add(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"Failed to export image for '{a.title}' to {dstPath}: {ex.Message}");
                        }
                    }
                }

                var category = a.projectParentCategory.ToString();
                var imagesJoined = string.Join("|", imageFileNames);

                // Escape semicolons by replacing with commas to keep format simple
                string Esc(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\r", " ").Replace("\n", " ").Replace(";", ",");

                writer.WriteLine(string.Join(";", new string[]
                {
                    Esc(a.title),
                    category,
                    imagesJoined,
                    Esc(a.shortDescription),
                    Esc(a.additionalInfo)
                }));
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(csvPath);
        Debug.Log($"Exported KielRegion CSV with {assets.Count} entries to: {csvPath}");
    }

    private static Texture2D GetReadableTexture(Texture2D source)
    {
        // Ensure we can read pixels
        var tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, tmp);
        var previous = RenderTexture.active;
        RenderTexture.active = tmp;
        var readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readable.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        return readable;
    }

        private static string SanitizeForFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "image";
            var s = input.Trim().ToLowerInvariant();
            s = s.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "");
            s = new string(System.Array.FindAll(s.ToCharArray(), ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' || ch == ' '));
            s = s.Replace(' ', '-');
            if (s.Length > 48) s = s.Substring(0, 48);
            if (string.IsNullOrEmpty(s)) s = "image";
            return s;
        }
}


