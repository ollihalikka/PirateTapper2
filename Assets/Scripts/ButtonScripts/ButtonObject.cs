using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonObject : MonoBehaviour
{

    public int Points = 1;
    public float SecondsToWaitAfterFail = 1f;
    public int playeri;
    public Wave Wave;
    private List<GameObject> EffectExplosion;
    private GameObject ButtonWrong;
    /// <summary>
    /// 1 is only one. 2 is one and one after that. For current one.
    /// </summary>
    public int HowManyButtonsLeft = 1;

    public Player Player;
    private bool clicked = false;
    public bool Activated = true;
    public Coroutine WaitForClicker;

    public void Init(Player player, int HowManyButtonsLeft, int Points, Wave wave,List<GameObject> effects,GameObject buttonWrong)
    {
        this.Player = player;
        this.HowManyButtonsLeft = HowManyButtonsLeft;
        this.Points = Points;
        this.Wave = wave;
        this.EffectExplosion = effects;
        this.ButtonWrong = buttonWrong;
        
        playeri = player.playerNumber == PlayerNumber.p1 ? 1 : 2;
    }
    public bool Shoot()
    {
        if (!Activated || !GetComponent<MeshRenderer>().enabled) //If not active, then return
            return false;
        
        Debug.Log("On Shoot down. this: " + HowManyButtonsLeft + ". nextBut: " + Player.NextNumber);
        if (HowManyButtonsLeft == 1 && Player.NextNumber == 1)
        {
            clicked = true;
            PointManager.instance.RemovePointsOnEnemy(Player, Points);
            Explode();
        }
        else if (HowManyButtonsLeft == Player.NextNumber)
        {
            Player.NextNumber -= 1;
            PointManager.instance.RemovePointsOnEnemy(Player, Points);
            Explode();
            Destroy(gameObject);
        }
        else
        {

            GameObject effect = Instantiate(ButtonWrong, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            
            //Player should press other button first, before pressing this one.
            if (GameManager.instance.useDamageFromIncorrectTap)
            {
                PointManager.instance.RemovePointsOnSelf(Player, Wave.DamageTakenFromIncorrectTap);
            }
            return false;
        }
        return true;
    }

    public IEnumerator WaitForClick(float maxTimer)
    {
        float timer = 0f;
        float maxTime = maxTimer;
        while (!clicked)
        {
            //Debug.Log("Waiting");
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
            if (timer > maxTime)
            {
                yield return TimerEnded();
                break;
            }
        }
       // Debug.Log("Done Waiting");
        Destroy(gameObject);
    }

    IEnumerator TimerEnded()
    {
        Debug.Log("You failed, Removing Health");
        foreach (var button in Player.PlayersButtons)
        {
            if (button == gameObject)
            {

                button.GetComponent<MeshRenderer>().enabled = false;
                button.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

                continue;
            }
            else
            {
                Destroy(button.gameObject);
            }
        }
        //Not Waiting Anymore.
        PointManager.instance.RemovePointsOnSelf(Player, Wave.DamageTakenFromAutoClear);
        yield return new WaitForSeconds(SecondsToWaitAfterFail);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Player != null)
        {
            Player.PlayersButtons.Remove(gameObject);
        }
        //Add sound effect and explosion effect
    }

    private void Explode()
    {
        GameObject effect = Instantiate(EffectExplosion[Random.Range(0, EffectExplosion.Count)], transform.position, Quaternion.identity);
        Destroy(effect, 2f);
    }
}
