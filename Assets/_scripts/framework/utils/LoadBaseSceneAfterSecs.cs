
using UnityEngine;
using UnityEngine.SceneManagement;


    public class LoadBaseSceneAfterSecs : MonoBehaviour
    {
        [SerializeField] WaitForSecondsAndCallback m_WaitForSeconds = default;

        private void OnValidate()
        {
            m_WaitForSeconds = GetComponent<WaitForSecondsAndCallback>();
        }

        private void Start()
        {
            m_WaitForSeconds.Prepare(3f, () =>
            {
                foreach (GameObject g in FindObjectsOfType<GameObject>())
                    Destroy(g);
                SceneManager.LoadScene(0);
            });
        }
    }
