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

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;

    [Header("Panel")]
    public GameObject backHomePanel;
    public GameObject winPanel;

    [Header("Buttons")]
    public Button buttonNextLevel;
    public Button buttonRestartLevel;
    public Button[] levelButtons;
    public TextMeshProUGUI[] levelButtonTexts;

    private GameObject currentLevel;
    private int currentLevelIndex = 0;

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
        SetupLevelButtons(); // Gán đúng index cho từng button
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void ShowHome()
    {
        canvasHome.SetActive(true);
        canvasSelectLevel.SetActive(false);
        canvasHowToPlay.SetActive(false);
        backHomePanel.SetActive(false);

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
        ShowWinButtons();
        UnlockNextLevel(); // ✅ Gọi đúng ở đây
        StartCoroutine(ShowWinPanelDelayed());
    }

    private IEnumerator ShowWinPanelDelayed()
    {
        yield return new WaitForSeconds(0.7f);
        winPanel.SetActive(true);
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
        canvasSelectLevel.SetActive(true);
        backHomePanel.SetActive(false);

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
        canvasHowToPlay.SetActive(true);
        PlayButtonClickSound();
    }

    public void BackToHomeFromHowToPlay() => ShowHome();

    public void BackToHomeFromSelectLevel() => ShowHome();

    public void LoadLevel(int levelIndex)
    {
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
            backHomePanel.SetActive(true);
            winPanel.SetActive(false);

            SetupLevelUIButtons();
            Debug.Log("Đã load level " + levelIndex);
        }
        else
        {
            Debug.LogError("Level index không hợp lệ!");
        }
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
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1; // Vì button 0 sẽ gọi LoadLevel(1)
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
        }
    }


    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs đã được reset");
    }
}
