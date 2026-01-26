using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ComboEncounter : MonoBehaviour
{
    const float CHANGE_COMBO_BG_SPEED = 25f;
    const float SCORE_ENCOUNT_DELAY_SECONDS = 1f;
    const float SCORE_ENCOUNT_SPEED_PER_SECOND = 100f;

    public TeamManager.Teams TrackedTeam = TeamManager.Teams.PLAYER;
    public float ResetComboDelay = 5f;
    public float ResetComboDelayOnStartLevel = 10f;

    private int _currentDisplayedScore = 0;
    private int _currentAddingScore = 0;
    private float _comboLastTime;
    private float _currentMultiplier = 1f;
    private Coroutine _setBgCoroutine = null;
    private Coroutine _scoreEncountCoroutine = null;

    [SerializeField] private List<ComboState> _comboStates = new();
    [SerializeField] private RectTransform _visualContainer;
    [SerializeField] private Image _comboBg;
    [SerializeField] private Image _oldComboBg;
    [SerializeField] private Image _comboLastTimeFillImage;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Serializable]
    public class ComboState
    {
        public int MinCombo;
        public int MaxCombo;
        public Sprite BgSprite;
        public float Multiplier = 1f;
    }

    public int CurrentCombo
    {
        get => ScoreManager.Instance.CurrentCombo;
        set
        {
            ScoreManager.Instance.CurrentCombo = value;
            _comboText.text = value.ToString();
            foreach (ComboState state in _comboStates)
            {
                if (state.MinCombo <= CurrentCombo && state.MaxCombo > CurrentCombo)
                {
                    CurrentMultiplier = state.Multiplier;

                    if (_comboBg.sprite != state.BgSprite)
                    {
                        if (_setBgCoroutine != null) StopCoroutine(_setBgCoroutine);
                        _setBgCoroutine = StartCoroutine(ShowNewBg(state.BgSprite));
                    }

                    return;
                }
            }


        }
    }

    private IEnumerator ShowNewBg(Sprite sprite)
    {
        _oldComboBg.sprite = _comboBg.sprite;
        _oldComboBg.transform.localScale = Vector3.one;
        _comboBg.sprite = sprite;
        _comboBg.transform.localScale = Vector3.zero;
        _comboBg.enabled = sprite != null;

        float progress = 0f;

        while (progress < 1f - 0.005f)
        {
            progress = math.lerp(progress, 1f, Time.deltaTime * CHANGE_COMBO_BG_SPEED);

            _oldComboBg.transform.localScale = Vector3.one * (1f - progress);
            _comboBg.transform.localScale = Vector3.one * progress;

            yield return new WaitForEndOfFrame();
        }
        _oldComboBg.transform.localScale = Vector3.zero;
        _comboBg.transform.localScale = Vector3.one;

        _setBgCoroutine = null;
    }

    public float CurrentMultiplier
    {
        get => _currentMultiplier;
        private set
        {
            _currentMultiplier = value;
            _multiplierText.text = "x" + _currentMultiplier.ToString();
        }
    }

    public void AddCombo()
    {
        CurrentCombo++;
        _comboLastTime = ResetComboDelay;
    }

    public void ResetCombo()
    {
        if (CurrentCombo > 0)
        {
            AddScore((int)math.round(CurrentCombo * CurrentMultiplier));
        }
        CurrentCombo = 0;
    }

    public void AddScore(int score)
    {
        ScoreManager.Instance.CurrentScore += score;
        _currentAddingScore += score;

        if (_scoreEncountCoroutine != null) StopCoroutine(_scoreEncountCoroutine);
        _scoreEncountCoroutine = StartCoroutine(ScoreEncount());
    }

    private IEnumerator ScoreEncount()
    {
        _scoreText.text = _currentAddingScore.ToString() + "+" + _currentDisplayedScore.ToString("00000");

        yield return new WaitForSeconds(SCORE_ENCOUNT_DELAY_SECONDS);

        while (_currentAddingScore > 0)
        {
            int scoreChange = (int)math.ceil(Time.deltaTime * SCORE_ENCOUNT_SPEED_PER_SECOND);
            if (_currentAddingScore < scoreChange) scoreChange = _currentAddingScore;

            _currentAddingScore -= scoreChange;
            _currentDisplayedScore += scoreChange;

            _scoreText.text = _currentAddingScore.ToString() + "+" + _currentDisplayedScore.ToString("00000");

            yield return new WaitForEndOfFrame();
        }

        _scoreText.text = _currentDisplayedScore.ToString("00000");
    }

    private void Awake()
    {
        _comboLastTime = CurrentCombo > 0 ? ResetComboDelayOnStartLevel : 0f;
        _currentDisplayedScore = ScoreManager.Instance.CurrentScore;
        _scoreText.text = _currentDisplayedScore.ToString("00000");
        CurrentCombo = CurrentCombo; //invokes text update

        TeamManager.Instance.GetTeamDataByTeam(TrackedTeam).OnTeamMemberDidKill += ComboEncounter_OnTeamMemberDidKill;
    }

    private void ComboEncounter_OnTeamMemberDidKill(object sender, CharacterTeam e)
    {
        AddCombo();
    }

    private void Update()
    {
        _comboLastTime -= Time.deltaTime;
        if (_comboLastTime < 0f)
        {
            _comboLastTime = 0f;
            if (CurrentCombo > 0) ResetCombo();
        }
        _comboLastTimeFillImage.fillAmount = _comboLastTime / ResetComboDelay;
    }

    private void OnDestroy()
    {
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.GetTeamDataByTeam(TrackedTeam).OnTeamMemberDidKill -= ComboEncounter_OnTeamMemberDidKill;
        }
    }
}