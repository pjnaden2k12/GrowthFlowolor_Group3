using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;

public class SwapBoxOnClick : MonoBehaviour
{
    public static event Action OnBoxSwapped;
    private GameObject firstBox = null;
    private GameObject secondBox = null;
    private Camera mainCamera;

    private Vector3 firstBoxOriginalPosition;
    private Vector3 secondBoxOriginalPosition;

    private bool isMoving = false;

    public float moveDuration = 0.5f;
    public float hoverHeight = 0.5f;
    public float hoverDuration = 0.2f;

    public AudioClip clickSound;  
    public AudioClip swapSound;   
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !isMoving)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("BoxMove"))
                {
                    SelectBox(hit.collider.gameObject);
                }
            }
        }
    }

    void SelectBox(GameObject selectedBox)
    {
        // Lần đầu: box phải có tag "BoxMove" và có ít nhất 1 child
        if (firstBox == null)
        {
            if (selectedBox.CompareTag("BoxMove") && selectedBox.transform.childCount > 0)
            {
                firstBox = selectedBox;
                firstBoxOriginalPosition = firstBox.transform.position;
                HoverBox(firstBox, true);
                PlayClickSound();
            }
            else
            {
                Debug.Log("Lần đầu chỉ được chọn box có ít nhất 1 child (tức là có Tower).");
            }
        }
        // Lần hai: chọn box khác có tag "BoxMove"
        else if (firstBox != selectedBox && selectedBox.CompareTag("BoxMove"))
        {
            secondBox = selectedBox;
            StartCoroutine(SwapBoxes());
        }
        // Nếu chọn lại box đầu tiên, hủy di chuyển
        else if (firstBox == selectedBox)
        {
            StartCoroutine(HoverBoxAsync(firstBox, false)); // Bay xuống
            firstBox.transform.position = firstBoxOriginalPosition; // Quay lại vị trí ban đầu
            firstBox = null;
        }
    }

    System.Collections.IEnumerator SwapBoxes()
    {
        // Cấm chọn thêm box khi đang di chuyển
        isMoving = true;

        // Box 2 bay lên trước
        yield return HoverBoxAsync(secondBox, true);

        Vector3 pos1 = firstBox.transform.position;
        Vector3 pos2 = secondBox.transform.position;

        PlaySwapSound();

        // Cùng di chuyển đổi chỗ
        Tween t1 = firstBox.transform.DOMove(pos2, moveDuration).SetEase(Ease.InOutQuad);
        Tween t2 = secondBox.transform.DOMove(pos1, moveDuration).SetEase(Ease.InOutQuad);
        yield return DOTween.Sequence().Append(t1).Join(t2).WaitForCompletion();

        // Bay xuống lại cả 2
        yield return HoverBoxAsync(firstBox, false);
        yield return HoverBoxAsync(secondBox, false);

        // Reset lại các box
        firstBox = null;
        secondBox = null;

        OnBoxSwapped?.Invoke();
        // Cho phép chọn lại khi di chuyển xong
        isMoving = false;
    }

    // Coroutine để hover lên/xuống và chờ tween xong
    System.Collections.IEnumerator HoverBoxAsync(GameObject box, bool up)
    {
        Vector3 targetPos = box.transform.position;
        targetPos.y += up ? hoverHeight : -hoverHeight;

        Tween t = box.transform.DOMoveY(targetPos.y, hoverDuration).SetEase(Ease.OutSine);
        yield return t.WaitForCompletion();
    }

    // Dùng cho hover đơn giản không chờ
    void HoverBox(GameObject box, bool up)
    {
        Vector3 targetPos = box.transform.position;
        targetPos.y += up ? hoverHeight : -hoverHeight;

        box.transform.DOMoveY(targetPos.y, hoverDuration).SetEase(Ease.OutSine);
    }
    void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound); 
        }
        else
        {
            Debug.LogWarning("Không có âm thanh cho việc nhấn chuột!");
        }
    }

    void PlaySwapSound()
    {
        if (swapSound != null)
        {
            audioSource.PlayOneShot(swapSound);  
        }
        else
        {
            Debug.LogWarning("Không có âm thanh cho việc đổi vị trí của box!");
        }
    }
}
