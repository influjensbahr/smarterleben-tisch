#if OTBT_AC
using UnityEngine;
using AC;
using OTBT.Framework.Utils;

#if UNITY_EDITOR
using UnityEditor;
using OTBT.Framework.Utils.Editor;
#endif

[System.Serializable]
public class DebugAction : Action
{
    public override ActionCategory Category => ActionCategory.OTBT;
    public override string Title => "Utils/Debug Log Output";
    public override string Description => "Prints text to the console.";

    [SerializeField] string m_Text;


#if UNITY_EDITOR
    public override void ShowGUI()
    {
        base.ShowGUI();
        m_Text = EditorGUILayout.TextField("Text", m_Text);
    }
    #endif

    public override float Run()
    {
        Dbg.Log(this, m_Text);
        return 0f;
    }
}
#endif
