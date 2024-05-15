using UnityEngine;
using UnityEngine.UI;

public class StarField : MonoBehaviour
{
    [SerializeField] public GameObject SternPrefab;
    public Canvas canvas;
    public float speed = 200f;
    public int starCount = 100;
    public Vector3 direction = new Vector3(1, 1, 0);
    public Vector3 offset = new Vector3(1, 1, 0);
    void Start()
    {
        for (int i = 0; i < starCount; i++)
        {
            SpawnStar();
        }
    }

    void Update()
    {
        foreach (Transform star in transform)
        {
            star.Translate(direction * speed * Time.deltaTime);

            if (star.position.x < 0 || star.position.y > Screen.height)
            {
                star.position = new Vector3(Random.Range(Screen.width, Screen.width * 2f), Random.Range(0, Screen.height * 1.5f), 0) + offset;
                var col = star.GetComponent<Image>().color;
                col.a = Random.Range(0f, 0.15f);
                star.GetComponent<Image>().color = col;
            }
        }
    }

    void SpawnStar()
    {
        GameObject star = Instantiate(SternPrefab, transform);
        star.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        star.GetComponent<RectTransform>().anchoredPosition = new Vector3(Random.Range(-Screen.width, 0), Random.Range(-Screen.height, 0), 0);
    }
}