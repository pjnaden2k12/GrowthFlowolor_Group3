using UnityEngine;

public class SkinSwitcher : MonoBehaviour
{
    public GameObject[] skinPrefabs;
    private GameObject currentSkin;

    void Start()
    {
        // Tìm SkinShopManager trong scene
        SkinShopManager manager = FindFirstObjectByType<SkinShopManager>();
        if (manager == null)
        {
            Debug.LogWarning("SkinShopManager not found!");
            return;
        }

        int idSkin = manager.GetEquippedSkinIndex();
        Debug.Log($"SkinSwitcher: loading skin id {idSkin} from SkinShopManager");

        SetSkin(idSkin);
    }

    public void SetSkin(int index)
    {
        if (index < 0 || index >= skinPrefabs.Length)
        {
            Debug.LogWarning($"Invalid skin index: {index}");
            return;
        }

        if (currentSkin != null)
            Destroy(currentSkin);

        currentSkin = Instantiate(skinPrefabs[index], transform);
        currentSkin.transform.localPosition = Vector3.zero;
        currentSkin.transform.localRotation = Quaternion.identity;
        currentSkin.SetActive(true);
    }
}
