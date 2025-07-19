using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStats playerStats;

    void Start()
    {
        Debug.Log("Level: " + playerStats.level);
        Debug.Log("EXP: " + playerStats.exp);
        Debug.Log("Gold: " + playerStats.gold);
    }

    public void GainExp(int amount)
    {
        playerStats.AddExp(amount);
    }

    public void GainGold(int amount)
    {
        playerStats.AddGold(amount);
    }

    public void SpendGold(int amount)
    {
        playerStats.SpendGold(amount);
    }
}
