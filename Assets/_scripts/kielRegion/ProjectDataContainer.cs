using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectDataContainer : MonoBehaviour
{
    [SerializeField] KielRegionProjectDataObject[] m_projectDataObjects;
    
    public List<KielRegionProjectDataObject> GetProjectsByCategory(ProjectCategory category)
    {
        return m_projectDataObjects.Where(projectDataObject => projectDataObject.projectParentCategory == category).ToList();
    }
}
