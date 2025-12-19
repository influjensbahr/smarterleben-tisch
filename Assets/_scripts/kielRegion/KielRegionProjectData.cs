using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime data container for a single project entry, typically loaded from CSV.
/// </summary>
public class KielRegionProjectData
{
    public uint id;
    public string title;
    public List<Sprite> projectImages = new List<Sprite>();
    public ProjectCategory projectParentCategory;
    public string shortDescription;
    public string additionalInfo;
}


