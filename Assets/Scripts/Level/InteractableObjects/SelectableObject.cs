using TMPro;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    const float EXTRA_INFO_POS_Z = -1;

    [Header("Selectable")]
    /// <summary>
    /// Used to make AI bettwe undertand how to interact with object
    /// </summary>

    public float SelectMaxRangeMultiplier = 1f; //value between 0 and 1
    public float SelectInfoTextOffset = 0.5f;

    [SerializeField] private GameObject _selectInfoContainer;
    [SerializeField] private SpriteRenderer _selectOutlineSprite;
    [SerializeField] private TextMeshProUGUI _selectText;

    protected SpriteRenderer _spriteRendererComponent;

    private bool _selected = false;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (!gameObject.activeInHierarchy || _selected == value) return;

            _selected = value;
            UpdateSelectInfo();
        }
    }

    protected virtual string GetSelectInfoText()
    {
        return "";
    }

    protected virtual bool SelectInfoAppearCondition()
    {
        return true;
    }

    private void OnDisable()
    {
        Selected = false;
    }

    private void Awake()
    {
        OnAwake();
    }

    private void Update()
    {
        UpdateSelectInfo();
    }

    private void UpdateSelectInfo()
    {
        if (Selected && SelectInfoAppearCondition())
        {
            if (_selectOutlineSprite != null)
            {
                _selectOutlineSprite.sprite = _spriteRendererComponent.sprite;
                _selectOutlineSprite.sortingOrder = _spriteRendererComponent.sortingOrder - 1;
                _selectOutlineSprite.flipX = _spriteRendererComponent.flipX;
                _selectOutlineSprite.flipY = _spriteRendererComponent.flipY;
            }

            if (_selectText != null)
            {
                _selectText.text = GetSelectInfoText();
                _selectText.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + SelectInfoTextOffset,
                    transform.position.z + EXTRA_INFO_POS_Z
                    );
                _selectText.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(Vector2.right);
            }

            if (_selectInfoContainer != null) _selectInfoContainer.SetActive(true);
        }
        else if (_selectInfoContainer != null)
        {
            _selectInfoContainer.SetActive(false);
        }
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _spriteRendererComponent)) throw new UnityException("SpriteRenderer component not found");
    }
}
