using System.Collections.Generic;
using UnityEngine;

public class ProjectDataContainer : MonoBehaviour
{
    [SerializeField] private KielRegionProjectDataObject[] m_projectDataObjects;
    
    public List<KielRegionProjectDataObject> GetProjectsByCategory(ProjectCategory category)
    {
        List<KielRegionProjectDataObject> projectsInCategory = new List<KielRegionProjectDataObject>();
        foreach (var projectDataObject in m_projectDataObjects)
        {
            if (projectDataObject.projectParentCategory == category)
            {
                projectsInCategory.Add(projectDataObject);
            }
        }
        return projectsInCategory;
    }
}
