using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PointManager : MonoBehaviour
{

    public static PointManager instance;
    
    public Transform p1HealthBar;
    public Transform p2HealthBar;

    public Canvas GameUI;
    public GameObject WinnerScreen;
    public GameObject P1IsWinner;
    public GameObject P2IsWinner;
    public GameObject UIHolder1;
    public GameObject UIHolder2;

    public MoveUiBarEffect p1UIEffectObject;
    public MoveUiBarEffect p2UIEffectObject;


    void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        UpdateTexts();
    }
    public void RemovePointsOnSelf(Player p, int amount)
    {
        DeductPointsOnPlayer(p, amount);
    }
    public void RemovePointsOnEnemy(Player ownPlayer, int am)
    {
        if(ownPlayer.playerNumber == PlayerNumber.p1)
        {
            DeductPointsOnPlayer(GameManager.instance.playerManager.player2, am);
        } else
        {
            DeductPointsOnPlayer(GameManager.instance.playerManager.player1, am);
        }
        
    }
    private void DeductPointsOnPlayer(Player p, int amount = 1)
    {
        if (!p.RemoveHealth(amount))
        {
            if (p.playerNumber == PlayerNumber.p1)
            {
                GameManager.instance.EndGame(GameManager.instance.playerManager.player2);
            }
            else
            {
                GameManager.instance.EndGame(GameManager.instance.playerManager.player1);
            }
        }
        if (p.playerNumber == PlayerNumber.p1)
        {
            if (p.Health >= 0)
            {
                float size = (float)p.Health / (float)GameManager.instance.MaxHealthForPlayers;
                Vector3 scale = new Vector3(size, 1, 1);
                p1HealthBar.localScale = scale;
                p1UIEffectObject.MoveBar(size);
                //Debug.Log("p1HealthBar.localScale: " + p1HealthBar.localScale + ". " + p.Health + "/" + GameManager.instance.MaxHealthForPlayers + " size : " + size);
            }
            else
            {
                p1HealthBar.gameObject.SetActive(false);
            }
        }
        else if (p.playerNumber == PlayerNumber.p2)
        {
            if (p.Health >= 0)
            {
                float size = (float)p.Health / (float)GameManager.instance.MaxHealthForPlayers;
                Vector3 scale = new Vector3(size, 1, 1);
                p2HealthBar.localScale = scale;
                p2UIEffectObject.MoveBar(size);
               // Debug.Log("p2HealthBar.localScale: " + p2HealthBar.localScale + ". " + p.Health + "/" + GameManager.instance.MaxHealthForPlayers + " size : " + size);
            }
            else
            {
                p2HealthBar.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateTexts()
    {
        p1HealthBar.localScale.Set(GameManager.instance.playerManager.player1.Health / GameManager.instance.MaxHealthForPlayers, 1, 1);
        p2HealthBar.localScale.Set(GameManager.instance.playerManager.player2.Health / GameManager.instance.MaxHealthForPlayers, 1, 1);
    }

    public void Victory(Player winner)
    {
        WinnerScreen.gameObject.SetActive(true);
        UIHolder1.SetActive(false);
        UIHolder2.SetActive(false);
        StopAllCoroutines();
        
        foreach (var a in GameManager.instance.playerManager.player1.PlayersButtons)
        {
            a.SetActive(false);
        }
        foreach (var a in GameManager.instance.playerManager.player2.PlayersButtons)
        {
            a.SetActive(false);
        }

        if (winner.playerNumber == PlayerNumber.p1)
        {
            P1IsWinner.gameObject.SetActive(true);
            P2IsWinner.gameObject.SetActive(false);
        }
        else
        {
            P2IsWinner.gameObject.SetActive(true);
            P1IsWinner.gameObject.SetActive(false);
        }
        //GameUI.gameObject.SetActive(false);
    }



}
