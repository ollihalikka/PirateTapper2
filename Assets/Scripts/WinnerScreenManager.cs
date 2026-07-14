using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerScreenManager : MonoBehaviour
{
    public RawImage Background;
    public float speed = 1;
    public GameObject activateTheseWhenDone;
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(fader());
    }

    IEnumerator fader()
    {
        Color color = Background.color;
        Color endColor = new Color(color.r, color.b, color.g, 1);
        float a = 0;
        while (a < 1)
        {
            Color x = new Color(color.r, color.b, color.g, a);
            Background.color = x;
            a += 0.001f * speed;
            yield return new WaitForSeconds(0.001f);
        }
        ActivateEverything();
    }


    void ActivateEverything()
    {
        PointManager.instance.GameUI.gameObject.SetActive(false);
        activateTheseWhenDone.SetActive(true);
        text.text = "Winner is PLAYER " + GameManager.instance.Winner.playerNumber;
    }


    public void OnExitButtonePress()
    {
        Application.Quit();
    }
    public void OnPlayAgainButtonPress()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void OnMenuButtonPress()
    {
        Debug.Log("Imagine going to main menu");
    }
}
