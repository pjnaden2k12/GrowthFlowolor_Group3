using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Audio")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Header("Effect")]
    public AudioClip bgmClip;
    private AudioSource bgmSource;

    [Header("Canvas")]
    public GameObject canvasHome;
    public GameObject canvasSelectLevel;
    public GameObject canvasHowToPlay;
    public GameObject fadePanelSelect;
    public GameObject canvasIngame;

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;

    [Header("Panel")]
    public GameObject backHomePanel;
    public GameObject winPanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI movesText;
    public GameObject losePanel;
    public GameObject questPanel;

    [Header("Buttons")]
    public Button buttonNextLevel;
    public Button buttonRestartLevel;
    public Button[] levelButtons;
    public TextMeshProUGUI[] levelButtonTexts;

    [Header("Shop")]
    public GameObject shopPanel;

    private GameObject currentLevel;
    private int currentLevelIndex = 0;
    private bool isLoadingLevel = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        ShowHome();
        HideWinButtons();
        LoadLevelUnlockStatus();
        SetupLevelButtons();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void ShowInGameHUD()
    {
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (movesText != null) movesText.gameObject.SetActive(true);
    }

    public void AnimatePanelIn(GameObject panel)
    {
        if (panel == null) return;
        panel.transform.DOKill(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
        panel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        cg.alpha = 0f;

        panel.SetActive(true);
        panel.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
        cg.DOFade(1f, 0.5f);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void ShowHome()
    {
        questPanel.SetActive(false);
        canvasHome.SetActive(true);
        canvasSelectLevel.SetActive(false);
        canvasHowToPlay.SetActive(false);
        backHomePanel.SetActive(false);
        shopPanel?.SetActive(false);
        if (canvasIngame != null) canvasIngame.SetActive(false);

        UnloadCurrentLevel();
        PlayButtonClickSound();

        if (bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }

        GameObject fadePanel = GameObject.Find("FadePanel");
        if (fadePanel)
        {
            CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>() ?? fadePanel.AddComponent<CanvasGroup>();
            fadePanel.SetActive(true);
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.DOFade(0f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cg.blocksRaycasts = false;
                fadePanel.SetActive(false);
            });
        }
    }

    public void ShowQuestPanel()
    {
        PlayButtonClickSound();
        if (questPanel != null)
        {
            AnimatePanelIn(questPanel);
        }
    }

    public void HideQuestPanel()
    {
        PlayButtonClickSound();
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null) audioSource.PlayOneShot(buttonClickSound);
    }

    private void ShowWinButtons()
    {
        buttonNextLevel.gameObject.SetActive(true);
        buttonRestartLevel.gameObject.SetActive(true);
    }

    public void OnGameWin()
    {
        HideInGameHUD();
        ShowWinButtons();
        UnlockNextLevel();
        StartCoroutine(ShowWinPanelDelayed());
    }

    public void HideInGameHUD()
    {
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (movesText != null) movesText.gameObject.SetActive(false);
    }

    private IEnumerator ShowWinPanelDelayed()
    {
        yield return new WaitForSeconds(0.7f);
        AnimatePanelIn(winPanel);
    }

    private void HideWinButtons()
    {
        buttonNextLevel.gameObject.SetActive(false);
        buttonRestartLevel.gameObject.SetActive(false);
    }

    public void ShowSelectLevelWithFade()
    {
        PlayButtonClickSound();
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(false);
        AnimatePanelIn(canvasSelectLevel);
        backHomePanel.SetActive(false);
        shopPanel?.SetActive(false);

        if (fadePanelSelect)
        {
            CanvasGroup cg = fadePanelSelect.GetComponent<CanvasGroup>() ?? fadePanelSelect.AddComponent<CanvasGroup>();
            fadePanelSelect.SetActive(true);
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.DOFade(0f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cg.blocksRaycasts = false;
                fadePanelSelect.SetActive(false);
            });
        }
    }

    public void Continue() => canvasSelectLevel.SetActive(true);

    public void ShowHowToPlay()
    {
        canvasHome.SetActive(false);
        AnimatePanelIn(canvasHowToPlay);
        PlayButtonClickSound();
    }

    public void BackToHomeFromHowToPlay() => ShowHome();

    public void BackToHomeFromSelectLevel() => ShowHome();

    public void LoadLevel(int levelIndex)
    {
        if (isLoadingLevel) return;
        isLoadingLevel = true;

        PlayButtonClickSound();
        UnloadCurrentLevel();

        if (levelIndex >= 0 && levelIndex < levelPrefabs.Length)
        {
            currentLevelIndex = levelIndex;
            currentLevel = Instantiate(levelPrefabs[levelIndex]);
            FindFirstObjectByType<GameManager>()?.ResetGame();

            canvasHome.SetActive(false);
            canvasSelectLevel.SetActive(false);
            canvasHowToPlay.SetActive(false);
            shopPanel?.SetActive(false);
            backHomePanel.SetActive(true);
            winPanel.SetActive(false);
            if (canvasIngame != null) canvasIngame.SetActive(true);

            ShowInGameHUD();
            SetupLevelUIButtons();
            Debug.Log("Đã load level " + levelIndex);
            UIQuestManager.Instance?.UpdateQuestProgress(1);
        }
        else
        {
            Debug.LogError("Level index không hợp lệ!");
        }

        isLoadingLevel = false;
    }

    private void SetupLevelUIButtons()
    {
        if (currentLevel == null) return;

        Button[] buttons = currentLevel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.CompareTag("ButtonHome"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ShowHome);
            }
            else if (btn.CompareTag("ButtonReplay"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => LoadLevel(currentLevelIndex));
            }
            else if (btn.CompareTag("ShopButton"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ShowShop);
            }
        }
    }

    private void UnloadCurrentLevel()
    {
        DOTween.KillAll();
        if (currentLevel != null) Destroy(currentLevel);
        currentLevel = null;
    }

    public void NextLevel()
    {
        if (currentLevelIndex + 1 < levelPrefabs.Length)
        {
            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("Đây là level cuối cùng!");
        }
    }

    public void RestartLevel() => LoadLevel(currentLevelIndex);

    private void UnlockNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < levelPrefabs.Length)
        {
            PlayerPrefs.SetInt("Level" + nextIndex, 1);
            PlayerPrefs.Save();
            Debug.Log("Mở khóa level " + nextIndex);
            LoadLevelUnlockStatus();
        }
    }

    private void LoadLevelUnlockStatus()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = PlayerPrefs.GetInt("Level" + i, i == 0 ? 1 : 0) == 1;
            levelButtons[i].interactable = unlocked;

            if (levelButtonTexts != null && i < levelButtonTexts.Length)
                levelButtonTexts[i].color = unlocked ? Color.blue : Color.red;
        }
    }

    private void SetupLevelButtons()
    {
        Debug.Log("SetupLevelButtons called");
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int capturedIndex = i;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(capturedIndex));
        }
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("Level0", 1);
        PlayerPrefs.Save();
        LoadLevelUnlockStatus();
        Debug.Log("PlayerPrefs đã được reset");
    }

    public void ShowShop()
    {
        PlayButtonClickSound();

        if (shopPanel != null)
        {
            AnimatePanelIn(shopPanel);
            canvasHome.SetActive(false);
            canvasHowToPlay.SetActive(false);
            canvasSelectLevel.SetActive(false);
            backHomePanel.SetActive(true);
        }
    }

    public void BackToHomeFromShop()
    {
        PlayButtonClickSound();
        shopPanel.SetActive(false);
        ShowHome();
    }
}
