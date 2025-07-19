using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BoxPoint : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public Transform pointSpawn;
    public Transform pointWin;

    public TowerType selectedTowerType;

    private GameObject currentTowerSpawn;
    private GameObject currentTowerWin;

    private HashSet<int> activeLayers = new HashSet<int>();  // Lưu các layer va chạm hiện tại

    private void Start()
    {
        SpawnTowerAtWinPoint(selectedTowerType);
        pointWin.DOScale(1.2f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        if (IsBoxMoveLayer(layer))
        {
            Tower towerInBox = other.GetComponentInChildren<Tower>();
            if (towerInBox != null)
            {
                activeLayers.Add(layer);  // Thêm layer vào danh sách va chạm
                UpdateTowerTypeBasedOnLayer();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int layer = other.gameObject.layer;
        if (IsBoxMoveLayer(layer))
        {
            activeLayers.Remove(layer);  // Xóa layer khỏi danh sách va chạm
            StartCoroutine(CheckNearbyBoxes());
        }
    }

    bool IsBoxMoveLayer(int layer)
    {
        return layer == LayerMask.NameToLayer("BoxMoveA") ||
               layer == LayerMask.NameToLayer("BoxMoveB") ||
               layer == LayerMask.NameToLayer("BoxMoveC") ||
               layer == LayerMask.NameToLayer("BoxMoveAB") ||
               layer == LayerMask.NameToLayer("BoxMoveAC") ||
               layer == LayerMask.NameToLayer("BoxMoveBC");
    }

    void UpdateTowerTypeBasedOnLayer()
    {
        TowerType newTowerType = selectedTowerType;

        if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveA")) &&
            activeLayers.Contains(LayerMask.NameToLayer("BoxMoveB")))
        {
            newTowerType = TowerType.AB;
        }
        else if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveA")) &&
                 activeLayers.Contains(LayerMask.NameToLayer("BoxMoveC")))
        {
            newTowerType = TowerType.AC;
        }
        else if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveB")) &&
                 activeLayers.Contains(LayerMask.NameToLayer("BoxMoveC")))
        {
            newTowerType = TowerType.BC;
        }
        else if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveA")))
        {
            newTowerType = TowerType.A;
        }
        else if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveB")))
        {
            newTowerType = TowerType.B;
        }
        else if (activeLayers.Contains(LayerMask.NameToLayer("BoxMoveC")))
        {
            newTowerType = TowerType.C;
        }

        if (newTowerType != selectedTowerType || currentTowerSpawn == null)
        {
            selectedTowerType = newTowerType;
            Invoke("CreateTowerWithDelay", 0.3f);  
        }
        else
        {
            ResetTowerIfNoCollisions();
        }

    }

    void ResetTowerIfNoCollisions()
    {
        if (activeLayers.Count == 0)  // Nếu không còn va chạm nào
        {
            if (currentTowerSpawn != null)
            {
                currentTowerSpawn.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Destroy(currentTowerSpawn);
                    selectedTowerType = TowerType.A;  // Reset về loại tower mặc định
                    CreateTower(selectedTowerType);  // Tạo lại tower mặc định
                });
            }
        }
    }

    void CreateTowerWithDelay()
    {
        CreateTower(selectedTowerType);
    }

    void CreateTower(TowerType type)
    {
        if (pointSpawn.childCount > 0)
        {
            Destroy(currentTowerSpawn);
        }

        if ((int)type >= towerPrefabs.Length || towerPrefabs[(int)type] == null) return;

        GameObject prefab = towerPrefabs[(int)type];
        currentTowerSpawn = Instantiate(prefab, pointSpawn.position, Quaternion.identity, pointSpawn);
        currentTowerSpawn.name = $"Tower_{type}";

        currentTowerSpawn.transform.localPosition = Vector3.zero;
        currentTowerSpawn.transform.localRotation = Quaternion.identity;
        currentTowerSpawn.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        currentTowerSpawn.transform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.OutBack);
    }

    void SpawnTowerAtWinPoint(TowerType type)
    {
        if (pointWin.childCount > 0) return;
        if ((int)type >= towerPrefabs.Length || towerPrefabs[(int)type] == null) return;

        GameObject prefab = towerPrefabs[(int)type];
        currentTowerWin = Instantiate(prefab, pointWin.position, Quaternion.identity, pointWin);
        currentTowerWin.name = $"TowerWin_{type}";
        currentTowerWin.transform.localPosition = Vector3.zero;
        currentTowerWin.transform.localRotation = Quaternion.identity;
        currentTowerWin.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(currentTowerWin.transform.DOScale(new Vector3(0.5f, 0.6f, 0.5f), 0.3f).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            currentTowerWin.transform.DOLocalRotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360)
                .SetLoops(-1).SetEase(Ease.Linear);
        });
    }

    IEnumerator CheckNearbyBoxes()
    {
        yield return new WaitForSeconds(0.3f);
        bool found = false;

        if (activeLayers.Count > 0)
        {
            found = true;
            UpdateTowerTypeBasedOnLayer();  // Cập nhật lại tower khi có va chạm
        }

        if (!found && currentTowerSpawn != null)
        {
            currentTowerSpawn.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Destroy(currentTowerSpawn);
            });
        }
    }
}
