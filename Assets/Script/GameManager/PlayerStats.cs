using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int level = 1;
    public int exp = 0;
    public int expToNextLevel = 100;
    public int gold = 0;

    private bool leveledUp = false;

    public void AddExp(int amount)
    {
        exp += amount;
        leveledUp = false;
        while (exp >= expToNextLevel)
        {
            exp -= expToNextLevel;
            level++;
            leveledUp = true;
        }
    }

    public bool HasLeveledUp()
    {
        if (leveledUp)
        {
            leveledUp = false; // reset cờ
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
    }
}
