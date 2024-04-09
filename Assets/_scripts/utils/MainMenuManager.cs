using OTBT.Framework.Core;
using OTBT.Framework.UI;
using UnityEngine;



    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_MainMenuScreen;
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_ErrorScreen;

        public void ShowErrorScreen()
        {
            if (m_ErrorScreen != null && !UIScreenController.instance.IsShowing(m_ErrorScreen))
                UIScreenController.instance.ShowScreen(m_ErrorScreen);
        }
        public void ShowNextScreen()
        {
            EventManager.instance.TriggerInTime(.6f, async () =>
            {
                    UIScreenController.instance.ShowScreen(m_MainMenuScreen);
            });
        }
    }

