using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStats playerStats;

    public TextMeshProUGUI levelUpText;
    public TextMeshProUGUI expGainText;
    public TextMeshProUGUI notificationText;

    private Queue<IEnumerator> notificationQueue = new Queue<IEnumerator>();
    private bool isShowingNotification = false;

    void Start()
    {
        levelUpText.gameObject.SetActive(true);
        expGainText.gameObject.SetActive(true);
        notificationText.gameObject.SetActive(false);

        Debug.Log("Level: " + playerStats.level);
        Debug.Log("EXP: " + playerStats.exp);
        Debug.Log("Gold: " + playerStats.gold);
    }

    public void GainExp(int amount)
    {
        EnqueueNotification(ShowExpGain(amount));
        playerStats.AddExp(amount);

        if (playerStats.HasLeveledUp())
        {
            EnqueueNotification(ShowLevelUp());
        }
    }

    public void GainGold(int amount)
    {
        playerStats.AddGold(amount);
        EnqueueNotification(ShowGoldGain(amount));
    }


    public void SpendGold(int amount)
    {
        playerStats.SpendGold(amount);
    }

    void EnqueueNotification(IEnumerator notificationRoutine)
    {
        notificationQueue.Enqueue(notificationRoutine);
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isShowingNotification = true;

        while (notificationQueue.Count > 0)
        {
            yield return StartCoroutine(notificationQueue.Dequeue());
        }

        isShowingNotification = false;
    }
    IEnumerator ShowExpGain(int amount)
    {
        expGainText.text = $"+{amount} EXP";
        expGainText.gameObject.SetActive(true);
        expGainText.DOKill();
        expGainText.DOFade(1f, 0.3f).From(0);

        if (notificationText != null)
        {
            notificationText.text = $"+{amount} EXP gained!";
            notificationText.gameObject.SetActive(true);
            notificationText.DOKill();
            notificationText.DOFade(1f, 0.3f).From(0);
        }

        yield return new WaitForSeconds(1.5f);

        expGainText.DOFade(0f, 0.3f);
        if (notificationText != null)
        {
            notificationText.DOFade(0f, 0.3f);
        }
    }

    IEnumerator ShowLevelUp()
    {
        int currentLevel = playerStats.level;
        int bonusGold = 100;

        GainGold(bonusGold);

        levelUpText.text = $"UP LEVEL {currentLevel} ( BONUS: {bonusGold} GOLD 0 )";
        levelUpText.gameObject.SetActive(true);
        levelUpText.DOKill();
        levelUpText.DOFade(1f, 0.3f).From(0);

        if (notificationText != null)
        {
            notificationText.text = $"Level Up! You reached level {currentLevel}!";
            notificationText.gameObject.SetActive(true);
            notificationText.DOKill();
            notificationText.DOFade(1f, 0.3f).From(0);
        }

        yield return new WaitForSeconds(2f);

        levelUpText.DOFade(0f, 0.3f);
        if (notificationText != null)
        {
            notificationText.DOFade(0f, 0.3f);
        }
    }

    IEnumerator ShowGoldGain(int amount)
    {
        if (notificationText != null)
        {
            notificationText.text = $"+{amount} GOLD received!";
            notificationText.gameObject.SetActive(true);
            notificationText.DOKill();
            notificationText.DOFade(1f, 0.3f).From(0);

            yield return new WaitForSeconds(1.5f);

            notificationText.DOFade(0f, 0.3f);
        }
    }

}
