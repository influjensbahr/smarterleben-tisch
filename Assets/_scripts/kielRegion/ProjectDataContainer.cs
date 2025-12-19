using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using OTBT.Framework.Utils;

/// <summary>
/// Central access point for project data and display names loaded from StreamingAssets.
/// </summary>
public class ProjectDataContainer : Singleton<ProjectDataContainer>
{
    [SerializeField] string m_CsvFileName = "projects.csv";
    [SerializeField] string m_CategoriesCsvFileName = "categories.csv";
    List<KielRegionProjectData> m_projects = new List<KielRegionProjectData>();
    Dictionary<ProjectCategory, string> m_categoryDisplayNames = new Dictionary<ProjectCategory, string>();

    void Awake()
    {
        m_projects = KielRegionCsvLoader.LoadFromStreamingAssets(m_CsvFileName);
        LoadCategoryDisplayNames();
    }

    public List<KielRegionProjectData> GetProjectsByCategory(ProjectCategory category)
    {
        Debug.Log("Data fetch: have " + m_projects.Count + " projects loaded.");
        return m_projects.Where(projectDataObject => projectDataObject.projectParentCategory == category).ToList();
    }

    public string GetCategoryDisplayName(ProjectCategory category)
    {
        if (m_categoryDisplayNames.TryGetValue(category, out var name)) return name;
        switch (category)
        {
            case ProjectCategory.RegionaleDatenplattform: return "Digitale Dienste";
            case ProjectCategory.SmarteMobilitaet: return "Mobilität";
            case ProjectCategory.Quartiersentwicklung: return "Quartiersentwicklung";
            case ProjectCategory.KuestenUndMeeresschutz: return "Regionales Küstenmanagement";
            case ProjectCategory.Kompetenzaufbau: return "Kompetenzaufbau";
            case ProjectCategory.Beteiligung: return "Beteiligung";
            default: return category.ToString();
        }
    }

    void LoadCategoryDisplayNames()
    {
        m_categoryDisplayNames.Clear();
        var path = Path.Combine(Application.streamingAssetsPath, m_CategoriesCsvFileName);
        if (!File.Exists(path)) return;
        try
        {
            var lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (i == 0 && (line.StartsWith("key;") || line.StartsWith("Key;"))) continue;
                var cols = line.Split(';');
                if (cols.Length < 2) continue;
                var key = cols[0].Trim();
                var value = cols[1].Trim();
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;
                if (System.Enum.TryParse<ProjectCategory>(key, out var cat))
                {
                    m_categoryDisplayNames[cat] = value;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failed to load category display names: " + ex.Message);
        }
    }
}
