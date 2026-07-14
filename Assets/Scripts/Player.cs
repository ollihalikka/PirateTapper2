using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    public Player player1;
    public Player player2;
}

public class Player
{
    public PlayerNumber playerNumber { get; set; }
    public int NextNumber { get; set; }
    public Wave CurrentWave { get; set; }
    public int Health { get; set;  }

    public List<GameObject> PlayersButtons = new List<GameObject>();

    public Player(PlayerNumber playerNumber, int nextNumber, int Health)
    {
        this.playerNumber = playerNumber;
        NextNumber = nextNumber;
        this.Health = Health;
    }
    /// <summary>
    /// Returns is alive.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Boolean. If true player is alive. If false, player is dead. </returns>
    public bool RemoveHealth(int value)
    {
        Health -= value;
        if (Health <= 0)
        {
            return false;
        }
        return true;
    }
}

public enum PlayerNumber { p1, p2 };
public enum GameMode { OrderMode, FastClickMode }