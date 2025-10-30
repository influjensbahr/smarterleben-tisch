using System.Collections.Generic;
using UnityEngine;

public class KielRegionProjectData
{
    public uint id;
    public string title;
    public List<Sprite> projectImages = new List<Sprite>();
    public ProjectCategory projectParentCategory;
    public string shortDescription;
    public string additionalInfo;
}


