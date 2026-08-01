using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(20)]
public class CursorManager : MonoBehaviour
{
    const float EXTRA_MIN_DISTANCE_FOR_RANGED_WEAPON = 0.15f;
    const float CURSOR_POINT_SCALE_FOR_FULL_HD = 2.3f;
    public static readonly Vector2 CURSOR_HOTSPOT = new(16f, 16f);

    public static CursorManager Instance;

    public float PointRotationSpeed = 720f;
    public float KillPointDuration = 1f;

    public Texture2D DefaultCursor;
    public Texture2D EmptyCursor;
    public Texture2D RestrictCursor;
    public Texture2D DotCursorPoint;
    public Texture2D CrossCursorPoint;

    private RawImage _currentPointSprite = null;
    private Texture2D _currentCursorTexture = null;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 CursorManager per game");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (UIManager.Instance?.GetMainCanvas() == null) return;

        if (_currentPointSprite?.IsDestroyed() ?? true)
        {
            GameObject newCursorGO = new GameObject("CursorDot");
            newCursorGO.transform.SetParent(UIManager.Instance.GetMainCanvas().transform);
            newCursorGO.transform.SetAsLastSibling();
            newCursorGO.transform.localScale = Vector2.one / CURSOR_POINT_SCALE_FOR_FULL_HD * (Screen.height / 1080f);
            _currentPointSprite = newCursorGO.AddComponent<RawImage>();
        }


        if (SceneList.GetCurrentSceneIsGameplay() && !UIManager.GamePaused())
        {
            if (TeamManager.Instance?.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers().Count == 1)
            {
                CharacterComponentsManager trackedCharacter = TeamManager.Instance?.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers()[0].CharComponents;

                if (trackedCharacter.CharacterHolding.CurrentHoldObject == null)
                {
                    SetCursor(null, EmptyCursor);
                }
                else if (trackedCharacter.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rw))
                {
                    if (rw.GetIsOutOfAmmo())
                    {
                        SetCursor(null, RestrictCursor);
                    }
                    else
                    {
                        SetCursor(CrossCursorPoint, EmptyCursor);
                    }
                }
                else
                {
                    SetCursor(DotCursorPoint, EmptyCursor);
                }

                float distanceToCamera = Vector2.Distance(
                    Camera.main.ScreenToWorldPoint(VectorMath.Vec2ToVec3(Mouse.current.position.ReadValue(), Mathf.Abs(Camera.main.transform.position.z - trackedCharacter.transform.position.z))),
                    trackedCharacter.Center.transform.position
                    );
                if (trackedCharacter.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rangedWeapon) ?? false)
                {
                    distanceToCamera = 
                        Mathf.Max(distanceToCamera, Vector2.Distance(trackedCharacter.Center.transform.position, rangedWeapon.ProjectileSpawnPosition.position) + EXTRA_MIN_DISTANCE_FOR_RANGED_WEAPON);
                }

                _currentPointSprite.transform.position =
                    Camera.main.WorldToScreenPoint(trackedCharacter.Center.transform.position + VectorMath.Vec2ToVec3(trackedCharacter.CharacterAiming.GetCurrentAimNormalized()) * distanceToCamera);

                if (trackedCharacter.CharacterReloading.GetIsReloading())
                {
                    _currentPointSprite.transform.Rotate(0f, 0f, PointRotationSpeed * Time.unscaledDeltaTime);
                }
                else
                {
                    _currentPointSprite.transform.rotation = Quaternion.identity;
                }
            }
            else
            {
                SetCursor(DotCursorPoint, EmptyCursor);
                _currentPointSprite.transform.position = Mouse.current.position.ReadValue();
            }
        }
        else
        {
            SetCursor(null, DefaultCursor);
        }
    }

    private void SetCursor(Texture2D point, Texture2D cursor)
    {
        if (_currentCursorTexture != cursor)
        {
            if (cursor == null)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.SetCursor(cursor, CURSOR_HOTSPOT, CursorMode.Auto);
                Cursor.visible = true;
            }
            _currentCursorTexture = cursor;
        }

        if (point == null)
        {
            _currentPointSprite.enabled = false;
        }
        else
        {
            _currentPointSprite.enabled = true;
            _currentPointSprite.texture = point;
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}