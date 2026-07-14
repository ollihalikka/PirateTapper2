using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public int alpa;
    public GameObject line;
    public List<Touch> playerOneTouches = new List<Touch>();
    public List<Touch> playerTwoTouches = new List<Touch>();
    public float lineWidth = 2f;
    public RectTransform GetLineWidth;
    public static InputManager instance;

    public GameObject ButtonNotClick;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (line == null)
        {
            line = new GameObject();
            line.AddComponent<LineRenderer>();
        }

        //Calculate the width of the line if set bigger or smaller.
        //Later you can add ability to move the line
        lineWidth = GetWidth();
    }

    // Update is called once per frame
    void Update()
    {


        //Divide multitouch inputs to two different lists. One for player one and one for player two. Player one is y coordinates top side of the screen. Player two is y coordinates bottom side of the screen.
        //If unity editor is used, count mousetouches aswell for debugging.
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {

            var touchposition = Input.mousePosition;

            // Shoot a raycast from the touch position
            Ray ray = Camera.main.ScreenPointToRay(touchposition);
            RaycastHit hit;

            // Check if the raycast hits an object with the "Enemy" tag
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Button"))
                {
                    // The raycast hit an enemy object, do something here
                    hit.transform.gameObject.GetComponent<ButtonObject>().Shoot();
                }
                else if (hit.transform.CompareTag("PlaneP1"))
                {
                    PointManager.instance.RemovePointsOnSelf(GameManager.instance.playerManager.player1, GameManager.instance.playerManager.player1.CurrentWave.DamageTakenFromIncorrectTap);
                    ButtonWrongPlay(hit.point);
                }
                else if (hit.transform.CompareTag("PlaneP2"))
                {
                    PointManager.instance.RemovePointsOnSelf(GameManager.instance.playerManager.player2, GameManager.instance.playerManager.player2.CurrentWave.DamageTakenFromIncorrectTap);
                    ButtonWrongPlay(hit.point);
                }
                else if (hit.transform.CompareTag("UI_Button"))
                {
                    if (hit.transform.name == "PlayAgainButton")
                    {
                        SceneManager.LoadScene(0);
                    }
                    else if (hit.transform.name == "QuitGameButton")
                    {
                        Application.Quit();
                    }
                }
            }
            // Debug draw the raycast
            if (GameManager.instance.DEBUG)
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f);
            }

        }
#endif


        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    //Started to touch

                    // Check if the touch position is within the screen bounds
                    if (touch.position.x >= 0 && touch.position.x <= Screen.width && touch.position.y >= 0 && touch.position.y <= Screen.height)
                    {
                        // Shoot a raycast from the touch position
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hit;

                        // Check if the raycast hits an object with the "Enemy" tag
                        if (Physics.Raycast(ray, out hit))
                        {
                            if (hit.transform.CompareTag("Button"))
                            {
                                // The raycast hit an enemy object, do something here
                                hit.transform.gameObject.GetComponent<ButtonObject>().Shoot();
                            }
                            else if (hit.transform.CompareTag("PlaneP1"))
                            {
                                PointManager.instance.RemovePointsOnSelf(GameManager.instance.playerManager.player1, GameManager.instance.playerManager.player1.CurrentWave.DamageTakenFromIncorrectTap);
                                ButtonWrongPlay(hit.point);
                            }
                            else if (hit.transform.CompareTag("PlaneP2"))
                            {
                                PointManager.instance.RemovePointsOnSelf(GameManager.instance.playerManager.player2, GameManager.instance.playerManager.player2.CurrentWave.DamageTakenFromIncorrectTap);
                                ButtonWrongPlay(hit.point);
                            }
                            else if (hit.transform.CompareTag("UI_Button"))
                            {
                                if (hit.transform.name == "PlayAgainButton")
                                {
                                    SceneManager.LoadScene(0);
                                }
                                else if (hit.transform.name == "QuitGameButton")
                                {
                                    Application.Quit();
                                }
                            }
                        }
                        // Debug draw the raycast
                        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 5);
                    }
                }
            }
        }

    }
    private void ButtonWrongPlay(Vector3 pos)
    {
        GameObject a = Instantiate(ButtonNotClick, pos, Quaternion.identity);
        Destroy(a, 1f);
    }
    public float GetWidth()
    {
        return GetLineWidth.lossyScale.y * GetLineWidth.rect.height;
    }

}

