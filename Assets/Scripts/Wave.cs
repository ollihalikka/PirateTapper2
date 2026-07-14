using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave
{
    public int WaveNumber { get; set; }
    public int NumberOfButtons { get; set; }
    public float TimeRequiredForWaveToForm { get; set; }
    public float TimeFromWaveStartToAutomaticClear { get; set; }
    public int DamageToEnemyPerSmashedButton { get; set; }
    public int DamageToEnemyPerWave { get; set; }
    public int DamageTakenFromAutoClear { get; set; }
    public int DamageTakenFromIncorrectTap { get; set; }

    public Wave(int waveNumber, int numberOfButtons, float timeRequiredForWaveToForm, float timeFromWaveStartToAutomaticClear, int damageToEnemyPerSmashedButton, int damageToEnemyPerWave, int damageTakenFromAutoClear, int damageTakenFromIncorrectTap)
    {
        WaveNumber = waveNumber;
        NumberOfButtons = numberOfButtons;
        TimeRequiredForWaveToForm = timeRequiredForWaveToForm;
        TimeFromWaveStartToAutomaticClear = timeFromWaveStartToAutomaticClear;
        DamageToEnemyPerSmashedButton = damageToEnemyPerSmashedButton;
        DamageToEnemyPerWave = damageToEnemyPerWave;
        DamageTakenFromAutoClear = damageTakenFromAutoClear;
        DamageTakenFromIncorrectTap = damageTakenFromIncorrectTap;
    }
}
