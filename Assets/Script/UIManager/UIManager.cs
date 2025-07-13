using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
    }

    public void ShowHome()
    {
        canvasHome.SetActive(true);
        canvasSelectLevel.SetActive(false);
        canvasHowToPlay.SetActive(false);

        ResetGameState();
        UnloadCurrentLevel();

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
    public void ShowSelectLevelWithFade()
    {
        PlayButtonClickSound();
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(false);
        canvasSelectLevel.SetActive(true);


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

    private void ResetGameState()
    {

    }
}
