using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DifficultyUI : MonoBehaviour
{
    public float TimeLineTimeCapacitySeconds = 180f; //3 mins

    [SerializeField] private RectTransform _stagesContainer;
    [SerializeField] private RectTransform _timeLineSizeContainer;
    [SerializeField] private DifficultyUIItem _itemInstance;
    [SerializeField] private TextMeshProUGUI _bottomInfoText;

    private List<DifficultyUIItem> _items = new();
    private int _currentItemIter = 0;

    private void Update()
    {
        if (DifficultyManager.Instance == null) return;

        ClearAllTimelines();

        float currentStageTime = 0;
        foreach (DifficultyManager.DifficultyStage stage in DifficultyManager.Instance.Difficulties)
        {
            for (int i = 1; i <= stage.MidstageCursesAmount; i++)
            {
                TryAddTimelineItem(
                    currentStageTime + i * (stage.Duration / (stage.MidstageCursesAmount + 1)),
                    stage.MidCurseIcon,
                    ""
                    );
            }

            TryAddTimelineItem(currentStageTime, stage.DifficultyIcon, stage.LocalizedName?.GetLocalizedString());

            currentStageTime += stage.Duration;
        }

        foreach (DifficultyUIItem item in _items)
        {
            if (item.gameObject.activeSelf != item.IsUsedInTimeline)
            {
                item.gameObject.SetActive(item.IsUsedInTimeline);
            }
        }

        TimeSpan time = new(0, 0, (int)math.floor(DifficultyManager.Instance.CurrentLoopDifficultyTime));
        _bottomInfoText.text =
            time.ToString(@"mm\:ss") + " | " +
            DifficultyManager.Instance.CurrentDifficulty.Value.LocalizedName?.GetLocalizedString();
    }

    private bool TryAddTimelineItem(float time, Sprite iconSprite, string titleText)
    {
        if (
            time > DifficultyManager.Instance.CurrentLoopDifficultyTime &&
            time < DifficultyManager.Instance.CurrentLoopDifficultyTime + TimeLineTimeCapacitySeconds
            )
        {
            if (_items.Count <= _currentItemIter)
            {
                _items.Add(Instantiate(_itemInstance, _stagesContainer));
            }

            _items[_currentItemIter].IconImage.sprite = iconSprite;
            _items[_currentItemIter].TitleText.text = titleText;

            _items[_currentItemIter].transform.position = new Vector3(
                _stagesContainer.position.x,
                _stagesContainer.position.y -
                    (_stagesContainer.rect.height - _stagesContainer.rect.width) / 2 +
                    (time - DifficultyManager.Instance.CurrentLoopDifficultyTime) / TimeLineTimeCapacitySeconds *
                    (_stagesContainer.rect.height - _stagesContainer.rect.width),
                _stagesContainer.transform.position.z
                );

            _items[_currentItemIter].IsUsedInTimeline = true;

            _currentItemIter++;

            return true;
        }
        else
        {
            return false;
        }
    }

    private void ClearAllTimelines()
    {
        foreach (var item in _items)
        {
            item.IsUsedInTimeline = false;
        }
        _currentItemIter = 0;
    }
}
