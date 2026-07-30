using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class SelectableObject : MonoBehaviour
{
    const float SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER = 5f;
    const float SELECTED_COLOR_DARKNESS = 0.64f;
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
            if (!gameObject.activeInHierarchy) return;

            _selected = value;
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

    private void Awake()
    {
        OnAwake();
    }

    private void Update()
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
