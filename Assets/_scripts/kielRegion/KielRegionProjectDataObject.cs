using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "KielRegionProjectDataObject", menuName = "KielRegion/Data/KielRegionProjectDataObject", order = 1)]
public class KielRegionProjectDataObject : ScriptableObject
{
    public uint id;
    public string title;
    public List<Sprite> projectImage;
    public ProjectCategory projectParentCategory;
    [TextArea(3, 10)] public string shortDescription;
    [TextArea(3, 10)] public string additionalInfo;
    public Vector2 projectLocation;
    
    public void SetProjectLocation(Vector3 position)
    {
        projectLocation = new Vector2(position.x, position.y);
    }
}
