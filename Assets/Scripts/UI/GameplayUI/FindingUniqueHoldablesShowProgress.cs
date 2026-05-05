using UnityEngine;
using UnityEngine.UI;

public class FindingUniqueHoldablesShowProgress : MonoBehaviour
{
    public FindUniqueHoldablesUnlockCondition Mission;
    public Material FoundHoldableMaterial;
    public Material NotFoundHoldableMaterial;

    private void OnEnable()
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        foreach (Holdable requiredHoldable in Mission.RequiredFindHoldables)
        {
            Sprite requiredHoldableSprite = requiredHoldable.GetComponent<SpriteRenderer>()?.sprite;

            GameObject newGO = new GameObject(requiredHoldable.FindingUniqueCodeName);
            newGO.transform.SetParent(transform, false);
            newGO.transform.localScale = requiredHoldableSprite.rect.size / requiredHoldableSprite.pixelsPerUnit;

            RectTransform newGORect = newGO.AddComponent<RectTransform>();

            Image newGOImage = newGO.AddComponent<Image>();
            newGOImage.sprite = requiredHoldableSprite;
            newGOImage.material =
                (SessionManager.Instance?.CurrentSession?.FoundUniqueHoldables.Contains(requiredHoldable.FindingUniqueCodeName) ?? false) ?
                FoundHoldableMaterial : NotFoundHoldableMaterial;
        }
    }
}