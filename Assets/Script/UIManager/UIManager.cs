using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Audio")]
    public AudioClip buttonClickSound;  
    private AudioSource audioSource;

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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        ShowHome();
        HideWinButtons();
        LoadLevelUnlockStatus();
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
            bgmSource.loop = true;
            bgmSource.Play();
        }

        GameObject fadePanel = GameObject.Find("FadePanel");
        if (fadePanel == null) return;

        CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = fadePanel.AddComponent<CanvasGroup>();
        }

        fadePanel.SetActive(true);
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        cg.DOFade(0f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            cg.blocksRaycasts = false;
            fadePanel.SetActive(false);
        });
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);  
        }
        else
        {
            Debug.LogWarning("Không có âm thanh cho Button Click!");
        }
    }
    private void ShowWinButtons()
    {
        buttonNextLevel.gameObject.SetActive(true);  
        buttonRestartLevel.gameObject.SetActive(true);  
    }
    public void OnGameWin()
    {
        ShowWinButtons();  
        winPanel.gameObject.SetActive(true);
        UnlockNextLevel();
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

        if (fadePanelSelect != null)
        {
            CanvasGroup cg = fadePanelSelect.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = fadePanelSelect.AddComponent<CanvasGroup>();

            fadePanelSelect.SetActive(true);
            cg.alpha = 1f;
            cg.blocksRaycasts = true;

            cg.DOFade(0f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cg.blocksRaycasts = false;
                fadePanelSelect.SetActive(false); 
            });
        }
        else
        {
            Debug.LogWarning("Không tìm thấy FadePanelSelect!");
        }
    }


    public void Continue() 
    {
        canvasHome.SetActive(false);
        canvasSelectLevel.SetActive(true);
    }

    public void ShowHowToPlay()
    {
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(true);
        PlayButtonClickSound();
    }

    public void BackToHomeFromHowToPlay()
    {
        ShowHome();
        PlayButtonClickSound();
    }

    public void BackToHomeFromSelectLevel()
    {
        ShowHome();
        PlayButtonClickSound();
    }

    public void LoadLevel(int levelIndex)
    {
        PlayButtonClickSound();
        UnloadCurrentLevel();

        if (levelIndex >= 0 && levelIndex < levelPrefabs.Length)
        {
            currentLevelIndex = levelIndex;
            currentLevel = Instantiate(levelPrefabs[levelIndex]);

            canvasHome.SetActive(false);
            canvasSelectLevel.SetActive(false);
            canvasHowToPlay.SetActive(false);
            backHomePanel.SetActive(true);
            winPanel.SetActive(false);

            SetupLevelUIButtons();

            Debug.Log(" Đã load level " + levelIndex);
        }
        else
        {
            Debug.LogError(" Level index không hợp lệ!");
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
        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
        }
    }

    public void NextLevel()
    {
        Debug.Log("Next Level!");

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
    public void RestartLevel()
    {
        Debug.Log("Restart Level!");

        LoadLevel(currentLevelIndex);
    }
    private void UnlockNextLevel()
    {
        if (currentLevelIndex + 1 < levelPrefabs.Length)
        {
            PlayerPrefs.SetInt("Level" + (currentLevelIndex + 1), 1);  
            PlayerPrefs.Save();  
        }
    }

    private void LoadLevelUnlockStatus()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isLevelUnlocked = PlayerPrefs.GetInt("Level" + i, i == 0 ? 1 : 0) == 1;
            levelButtons[i].interactable = isLevelUnlocked;

            if (!isLevelUnlocked && levelButtonTexts != null && i < levelButtonTexts.Length)
            {
                levelButtonTexts[i].color = Color.red;
            }
            else if (levelButtonTexts != null && i < levelButtonTexts.Length)
            {
                levelButtonTexts[i].color = Color.blue;  
            }
        }
    }
}
