using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinShopManager : MonoBehaviour
{
    public PlayerStats playerStats;
    public SkinData[] skins;

    public Button[] skinButtons;
    public TextMeshProUGUI[] skinButtonTexts;

    private int equippedIndex = 0;

    void Start()
    {
        LoadData();

        for (int i = 0; i < skins.Length; i++)
        {
            int index = i;
            skinButtons[i].onClick.AddListener(() => OnSkinButtonClicked(index));
            UpdateButton(index);
        }

        EquipSkin(equippedIndex);
    }

    void OnSkinButtonClicked(int index)
    {
        Debug.Log($"Skin button {index} clicked");

        if (!skins[index].isUnlocked)
        {
            if (playerStats.gold >= skins[index].price)
            {
                playerStats.gold -= skins[index].price;
                skins[index].isUnlocked = true;
                SaveData();
                UpdateButton(index);
            }
        }
        else
        {
            EquipSkin(index);
            SaveData();
        }
    }

    void EquipSkin(int index)
    {
        equippedIndex = index;

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].isUnlocked)
            {
                skinButtonTexts[i].text = (i == index) ? "Equipped" : "Equip";
            }
            else
            {
                skinButtonTexts[i].text = skins[i].price + " Gold";
            }
        }
    }

    public int GetEquippedSkinIndex()
    {
        return equippedIndex;
    }

    void UpdateButton(int index)
    {
        if (skins[index].isUnlocked)
        {
            skinButtonTexts[index].text = (index == equippedIndex) ? "Equipped" : "Equip";
        }
        else
        {
            skinButtonTexts[index].text = skins[index].price + " Gold";
        }
    }

    void SaveData()
    {
        SkinSaveData data = new SkinSaveData();
        data.unlocked = new bool[skins.Length];
        for (int i = 0; i < skins.Length; i++)
        {
            data.unlocked[i] = skins[i].isUnlocked;
        }
        data.equippedIndex = equippedIndex;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SkinSaveData", json);
        PlayerPrefs.Save();

        Debug.Log("Skin data saved");
    }

    void LoadData()
    {
        if (PlayerPrefs.HasKey("SkinSaveData"))
        {
            string json = PlayerPrefs.GetString("SkinSaveData");
            SkinSaveData data = JsonUtility.FromJson<SkinSaveData>(json);

            if (data.unlocked.Length == skins.Length)
            {
                for (int i = 0; i < skins.Length; i++)
                {
                    skins[i].isUnlocked = data.unlocked[i];
                }
                equippedIndex = data.equippedIndex;
            }
            else
            {
                Debug.LogWarning("SkinSaveData length mismatch");
            }
        }
        else
        {
            Debug.Log("No skin save data found, using default");
        }
    }
}

[System.Serializable]
public class SkinSaveData
{
    public bool[] unlocked;
    public int equippedIndex;
}

[System.Serializable]
public class SkinData
{
    public string skinName;
    public int price;
    public bool isUnlocked;
}
