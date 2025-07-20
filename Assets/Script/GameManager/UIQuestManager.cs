using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIQuestManager : MonoBehaviour
{
    public static UIQuestManager Instance;

    public Quest[] quests = new Quest[3];

    public TextMeshProUGUI[] questTexts;
    public Button[] claimButtons;

    public Button dailyBonusButton;
    public TextMeshProUGUI dailyBonusText;

    public Button resetButton;

    private const string DATE_KEY = "LastQuestDate";
    private const string QUEST_DATA_KEY = "QuestData";
    private const string BONUS_CLAIMED_KEY = "DailyBonusClaimed";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadQuests();
        CheckResetByDate();
        UpdateUI();
        SetupDailyBonus();
        resetButton.onClick.AddListener(OnResetPressed);

    }
    void OnResetPressed()
    {
        PlayerPrefs.DeleteKey(QUEST_DATA_KEY);
        PlayerPrefs.DeleteKey(DATE_KEY);
        PlayerPrefs.DeleteKey(BONUS_CLAIMED_KEY);
        PlayerPrefs.Save();

        foreach (var quest in quests)
        {
            quest.Reset();
        }

        UpdateUI();
        SetupDailyBonus();

        Debug.Log("Dữ liệu nhiệm vụ và bonus đã được reset.");
    }

    void CheckResetByDate()
    {
        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (lastDate != today)
        {
            ResetQuests();
            PlayerPrefs.SetString(DATE_KEY, today);
            PlayerPrefs.SetInt(BONUS_CLAIMED_KEY, 0);
        }
    }

    void ResetQuests()
    {
        foreach (var quest in quests)
        {
            quest.Reset();
        }
        SaveQuests();
        UpdateUI();
    }

    public void UpdateQuestProgress(int questIndex, int amount = 1)
    {
        if (questIndex < 0 || questIndex >= quests.Length) return;
        if (quests[questIndex].isCompleted) return;

        quests[questIndex].currentProgress += amount;
        quests[questIndex].CheckCompleted();

        SaveQuests();
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < quests.Length && i < questTexts.Length; i++)
        {
            var q = quests[i];
            questTexts[i].text = $"{q.title}: {q.currentProgress}/{q.target}" + (q.isCompleted ? "" : "");

            if (claimButtons != null && i < claimButtons.Length)
            {
                bool shouldShowButton = q.isCompleted && !q.isClaimed;

                claimButtons[i].gameObject.SetActive(shouldShowButton);
                claimButtons[i].interactable = shouldShowButton;

                claimButtons[i].onClick.RemoveAllListeners();

                if (shouldShowButton)
                {
                    int index = i;
                    claimButtons[i].onClick.AddListener(() => OnClaimReward(index));
                }
            }
        }
    }

    public void OnClaimReward(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Length) return;

        Quest quest = quests[questIndex];
        if (!quest.isCompleted || quest.isClaimed) return;

        quest.isClaimed = true;

        PlayerStatsManager playerStatsManager = FindFirstObjectByType<PlayerStatsManager>();
        if (playerStatsManager != null)
        {
            playerStatsManager.GainExp(50);
        }

        SaveQuests();
        UpdateUI();
    }

    void SaveQuests()
    {
        string json = JsonUtility.ToJson(new QuestSaveWrapper(quests));
        PlayerPrefs.SetString(QUEST_DATA_KEY, json);
        PlayerPrefs.Save();
    }

    void LoadQuests()
    {
        if (PlayerPrefs.HasKey(QUEST_DATA_KEY))
        {
            string json = PlayerPrefs.GetString(QUEST_DATA_KEY);
            QuestSaveWrapper wrapper = JsonUtility.FromJson<QuestSaveWrapper>(json);
            if (wrapper.quests != null && wrapper.quests.Length == quests.Length)
            {
                quests = wrapper.quests;
            }
        }
    }

    void SetupDailyBonus()
    {
        int claimed = PlayerPrefs.GetInt(BONUS_CLAIMED_KEY, 0);
        dailyBonusButton.interactable = claimed == 0;
        dailyBonusText.text = claimed == 0 ? "Claim Daily Bonus" : "Bonus Claimed";
        dailyBonusButton.onClick.RemoveAllListeners();
        dailyBonusButton.onClick.AddListener(ClaimDailyBonus);
    }

    void ClaimDailyBonus()
    {
        PlayerStatsManager playerStatsManager = FindFirstObjectByType<PlayerStatsManager>();
        if (playerStatsManager != null)
        {
            playerStatsManager.GainGold(100);
            playerStatsManager.GainExp(20);
        }

        PlayerPrefs.SetInt(BONUS_CLAIMED_KEY, 1);
        PlayerPrefs.Save();

        dailyBonusButton.interactable = false;
        dailyBonusText.text = "Bonus Claimed";
    }
}

[System.Serializable]
public class QuestSaveWrapper
{
    public Quest[] quests;

    public QuestSaveWrapper(Quest[] _quests)
    {
        quests = _quests;
    }
}
