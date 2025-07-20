using UnityEngine;
using TMPro;

public class LevelConditionsManager : MonoBehaviour
{
    public float levelTimeInSeconds = 60f;
    public int maxMoves = 20;

    private TextMeshProUGUI timerText;
    private TextMeshProUGUI movesText;
    private GameObject losePanel;

    private float currentTime;
    private int currentMoves;
    private bool isLevelActive = true;
    private float levelStartTime;
    private bool levelWon = false;

    void Awake()
    {
        SwapBoxOnClick.OnBoxSwapped += OnPlayerMadeMove;
    }

    void Start()
    {
        if (UIManager.Instance != null)
        {
            timerText = UIManager.Instance.timerText;
            movesText = UIManager.Instance.movesText;
            losePanel = UIManager.Instance.losePanel;
        }
        else
        {
            Debug.LogError("UIManager.Instance not found! Cannot get UI references.");
            return;
        }

        currentTime = levelTimeInSeconds;
        currentMoves = maxMoves;
        isLevelActive = true;
        levelStartTime = Time.time;

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        UpdateTimerUI();
        UpdateMovesUI();
    }

    void Update()
    {
        if (!isLevelActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            LoseGame("Time's Up!");
        }

        UpdateTimerUI();
    }

    private void OnPlayerMadeMove()
    {
        if (!isLevelActive) return;

        currentMoves--;
        UpdateMovesUI();

        if (currentMoves <= 0 && !FindFirstObjectByType<GameManager>().gameWon)
        {
            LoseGame("Out of Moves!");
        }
    }

    private void LoseGame(string reason)
    {
        isLevelActive = false;
        Debug.Log($"Game Lost: {reason}");

        SwapBoxOnClick swapScript = FindFirstObjectByType<SwapBoxOnClick>();
        if (swapScript != null)
        {
            swapScript.enabled = false;
        }
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (movesText != null) movesText.gameObject.SetActive(false);

        if (losePanel != null)
        {
            UIManager.Instance.AnimatePanelIn(losePanel);
        }
    }

    public void OnLevelWin()
    {
        if (levelWon) return;
        levelWon = true;

        float timeTaken = Time.time - levelStartTime;

        if (timeTaken <= 10f)
        {
            UIQuestManager.Instance?.UpdateQuestProgress(2);
        }

        StopConditionsCheck();
    }

    public void StopConditionsCheck()
    {
        isLevelActive = false;
        Debug.Log("Level Won! Stopping timers and move counts.");
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateMovesUI()
    {
        if (movesText == null) return;
        movesText.text = $"Moves: {currentMoves}";
    }

    void OnDestroy()
    {
        SwapBoxOnClick.OnBoxSwapped -= OnPlayerMadeMove;
    }
}
