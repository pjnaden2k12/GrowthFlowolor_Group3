using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStats : ScriptableObject
{
    public int level = 1;
    public int exp = 0;
    public int gold = 0;

    public void AddExp(int amount)
    {
        exp += amount;

        while (exp >= 100)
        {
            exp -= 100;
            level++;
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public void SpendGold(int amount)
    {
        gold = Mathf.Max(0, gold - amount);
    }
}
