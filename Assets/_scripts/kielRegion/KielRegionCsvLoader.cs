using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class KielRegionCsvLoader
{
    // CSV schema (semicolon-delimited):
    // title;category;imageFiles;shortDescription;additionalInfo
    // imageFiles are pipe-separated filenames relative to Application.streamingAssetsPath + "/KielRegionImages"

    public static List<KielRegionProjectData> LoadFromStreamingAssets(string csvFileName = "projects.csv")
    {
        var results = new List<KielRegionProjectData>();
        var csvPath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        if (!FileExists(csvPath))
        {
            Debug.LogWarning($"KielRegion CSV not found at: {csvPath}");
            return results;
        }

        try
        {
            var lines = ReadAllLines(csvPath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (i == 0 && StartsWithHeader(line)) continue; // skip header if present

                var cols = SplitSemicolon(line);
                if (cols.Length < 5)
                {
                    Debug.LogWarning($"CSV line {i + 1} malformed (expected 5+ columns): {line}");
                    continue;
                }

                var data = new KielRegionProjectData();
                data.title = cols[0];
                data.projectParentCategory = ParseCategory(cols[1]);
                
                var imagesField = cols[2];
                if (!string.IsNullOrWhiteSpace(imagesField))
                {
                    var imgFiles = imagesField.Split('|');
                    foreach (var file in imgFiles)
                    {
                        var trimmed = file.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;
                        var sprite = LoadSpriteFromStreamingAssets(trimmed);
                        if (sprite != null) data.projectImages.Add(sprite);
                    }
                }
                
                data.shortDescription = cols.Length > 3 ? cols[3] : "";
                data.additionalInfo = cols.Length > 4 ? cols[4] : "";
                results.Add(data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read KielRegion CSV: {ex}");
        }

        Debug.Log("Startup: loaded " + results.Count + " results!");
        return results;
    }

    static bool StartsWithHeader(string line)
    {
        return line.StartsWith("title;") || line.StartsWith("Title;");
    }

    static string[] SplitSemicolon(string line)
    {
        // Simple split; assumes no escaped semicolons. For more complex needs, replace with a CSV parser.
        return line.Split(';');
    }

    static ProjectCategory ParseCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ProjectCategory.SmarteMobilitaet;

        // Try exact enum name first
        try
        {
            if (Enum.TryParse<ProjectCategory>(value, out var catExact)) return catExact;
        }
        catch { }

        // Normalize and map common display names (handle umlauts and punctuation)
        var norm = NormalizeCategory(value);
        switch (norm)
        {
            case "smartemobilitaet": return ProjectCategory.SmarteMobilitaet;
            case "quartiersentwicklung": return ProjectCategory.Quartiersentwicklung;
            case "kuestenundmeeresschutz": return ProjectCategory.KuestenUndMeeresschutz;
            case "kompetenzaufbau": return ProjectCategory.Kompetenzaufbau;
            case "beteiligung": return ProjectCategory.Beteiligung;
            case "digitaledienste": return ProjectCategory.RegionaleDatenplattform; // Display name used in UI
            case "regionaledatenplattform": return ProjectCategory.RegionaleDatenplattform;
        }

        return ProjectCategory.SmarteMobilitaet;
    }

    static string NormalizeCategory(string raw)
    {
        var s = raw.Trim().ToLowerInvariant();
        s = s.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
        s = s.Replace("-", "").Replace(" ", "").Replace("_", "");
        s = s.Replace("&", "und");
        return s;
    }

    static string[] ReadAllLines(string path)
    {
        // Read as UTF-8 explicitly to avoid mojibake for umlauts
        return File.ReadAllLines(path, System.Text.Encoding.UTF8);
    }

    static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    static Sprite LoadSpriteFromStreamingAssets(string fileName)
    {
        var imagesDir = Path.Combine(Application.streamingAssetsPath, "KielRegionImages");
        var fullPath = Path.Combine(imagesDir, fileName);
        if (!File.Exists(fullPath)) return null;
        try
        {
            var bytes = File.ReadAllBytes(fullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes)) return null;
            tex.name = fileName;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sprite.name = Path.GetFileNameWithoutExtension(fileName);
            return sprite;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to load sprite {fileName}: {ex.Message}");
            return null;
        }
    }
}


