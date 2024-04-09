using OTBT.Framework;
using OTBT.Framework.Core;
using OTBT.Framework.UI;
using Sparrow.Verification;
using UnityEngine;

[RequireComponent(typeof(UIScreen))]
public class MainMenu : MonoBehaviour, IVerify
{
    [SerializeField] UIScreen m_Screen;
    [SerializeField] StringOrAtomReference<SceneAtom> m_GameStartReference = null;
    [SerializeField] StringOrAtomReference<SceneAtom> m_MainMenuScreen;
    [SerializeField] StringOrAtomReference<UIScreenAtom> m_SceneSelectScreen;
    
    void OnValidate()
    {
        if (m_Screen == null) m_Screen = GetComponent<UIScreen>();
    }

    public void StartGame()
    {
        SceneLoadManager.instance.SwitchToScene(m_GameStartReference);
        UIScreenController.instance.HideScreen(m_Screen.title);
    }

    public void StartGame(SceneCollection scene)
    {
        SceneLoadManager.instance.SwitchToScene(scene.objectRef);
        UIScreenController.instance.HideScreen(m_Screen.title);
    }

    public void SceneSelectionPressed()
    {
        UIScreenController.instance.ShowScreen(m_SceneSelectScreen);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteAll();
        m_Screen.Hide();
        SceneLoadManager.instance.SwitchToScene(m_MainMenuScreen);
    }


    public void ShowCloudSaveOptions()
    {
        //_ = CloudSaveScreen.instance.Options(FBTServerDownload.playerToken);
    }

    public async void QuitGame()
    {
        //await SaveGame.instance.Save();
        Application.Quit();
    }

    public void Verify(CheckVerifyInterface checker)
    {
    }
}