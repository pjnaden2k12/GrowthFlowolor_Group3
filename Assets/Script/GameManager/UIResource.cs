using TMPro;
using UnityEngine;

public class UIResource : MonoBehaviour
{
    public PlayerStats playerStats;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;

    void Update()
    {
        levelText.text = "Level: " + playerStats.level;
        expText.text = "EXP: " + playerStats.exp + " / 100";
        goldText.text = "" + playerStats.gold;
    }
}
