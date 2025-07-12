using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private void Update()
    {
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
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
            Debug.Log("Game Win!");
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
}
