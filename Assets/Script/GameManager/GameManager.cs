using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public bool gameWon = false;
    public AudioClip winSound;  
    private AudioSource audioSource;
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); 
        audioSource.playOnAwake = false;  
    }
    private void Update()
    {
        if (GameObject.FindWithTag("BoxPoint") != null)
        {
            CheckWinCondition();
        }
    }

    void CheckWinCondition()
    {
        if (gameWon) return; // Đã thắng rồi thì không kiểm tra nữa

        bool allMatch = true;

        GameObject[] boxPoints = GameObject.FindGameObjectsWithTag("BoxPoint");

        foreach (var boxPointObj in boxPoints)
        {
            BoxPoint boxPoint = boxPointObj.GetComponent<BoxPoint>();

            if (boxPoint == null) continue;

            if (boxPoint.pointSpawn.childCount == 0 || boxPoint.pointWin.childCount == 0)
            {
                allMatch = false;
                continue;
            }

            var spawnTower = GetTowerInPoint(boxPoint.pointSpawn);
            var winTower = GetTowerInPoint(boxPoint.pointWin);

            if (spawnTower == null || winTower == null)
            {
                allMatch = false;
                continue;
            }

            if (spawnTower.towerType != winTower.towerType)
            {
                allMatch = false;
            }
        }

        if (allMatch)
        {
            gameWon = true;
            Debug.Log("Game Win!");
            var levelConditions = FindFirstObjectByType<LevelConditionsManager>();
            levelConditions?.OnLevelWin();
            PlayWinSound();
            UIManager.Instance.OnGameWin();
            UIQuestManager.Instance.UpdateQuestProgress(0);
            
        }
    }

    Tower GetTowerInPoint(Transform point)
    {
        foreach (Transform child in point)
        {
            Tower tower = child.GetComponent<Tower>();
            if (tower != null)
            {
                return tower;
            }
        }
        return null;
    }
    void PlayWinSound()
    {
        if (winSound != null)
        {
            audioSource.PlayOneShot(winSound); 
        }
        else
        {
            Debug.LogWarning("Không có âm thanh chiến thắng!");
        }
    }
    public void ResetGame()
    {
        gameWon = false;
    }

}
