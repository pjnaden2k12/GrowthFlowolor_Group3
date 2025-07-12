using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Effect")]
    public AudioClip bgmClip;       // Nhạc nền
    private AudioSource bgmSource;  // AudioSource để phát nhạc

    [Header("Canvas")]
    public GameObject canvasHome;          // Màn hình chính (start)
    public GameObject canvasSelectLevel;   // Màn hình chọn màn chơi
    public GameObject canvasHowToPlay;     // Màn hình hướng dẫn

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;

    private GameObject currentLevel;
    private int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tạo AudioSource để phát nhạc
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
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

        // Phát nhạc nền
        if (bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // Tìm FadePanel (nếu có thì fade, không thì bỏ qua im lặng)
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


    public void ShowSelectLevelWithFade()
    {
        // Ẩn các canvas khác
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(false);
        canvasSelectLevel.SetActive(true);

        // Tìm đúng FadePanel nằm trong canvasSelectLevel
        GameObject fadePanel = GameObject.Find("FadePanelSelect");

        if (fadePanel != null)
        {
            CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = fadePanel.AddComponent<CanvasGroup>();

            fadePanel.SetActive(true);
            cg.alpha = 1f;
            cg.blocksRaycasts = true;

            cg.DOFade(0f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cg.blocksRaycasts = false;
                fadePanel.SetActive(false); // Ẩn sau khi fade xong
            });
        }
        else
        {
            Debug.LogWarning("Không tìm thấy FadePanelSelect!");
        }
    }


    public void Continue() // nếu bạn vẫn muốn dùng cách này không có hiệu ứng
    {
        canvasHome.SetActive(false);
        canvasSelectLevel.SetActive(true);
    }

    public void ShowHowToPlay()
    {
        canvasHome.SetActive(false);
        canvasHowToPlay.SetActive(true);
    }

    public void BackToHomeFromHowToPlay()
    {
        ShowHome();
    }

    public void BackToHomeFromSelectLevel()
    {
        ShowHome();
    }

    public void LoadLevel(int levelIndex)
    {
        UnloadCurrentLevel();

        if (levelIndex >= 0 && levelIndex < levelPrefabs.Length)
        {
            currentLevelIndex = levelIndex;
            currentLevel = Instantiate(levelPrefabs[levelIndex]);

            canvasHome.SetActive(false);
            canvasSelectLevel.SetActive(false);
            canvasHowToPlay.SetActive(false);

            SetupLevelUIButtons();

            Debug.Log("🎮 Đã load level " + levelIndex);
        }
        else
        {
            Debug.LogError("⚠️ Level index không hợp lệ!");
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
        // Không còn thời gian hay điểm số, nên không cần reset gì nhiều
    }
}
