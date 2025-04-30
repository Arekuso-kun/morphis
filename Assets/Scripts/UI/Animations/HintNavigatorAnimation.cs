using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HintNavigatorAnimation : MonoBehaviour
{
    [SerializeField] private List<GameObject> _hintObjects;
    private List<RectTransform> _rectTransforms = new List<RectTransform>();
    private List<Vector2> _offScreenPositions = new List<Vector2>();
    private List<Vector2> _onScreenPositions = new List<Vector2>();

    private void Awake()
    {
        for (int i = 0; i < _hintObjects.Count; i++)
        {
            var rectTransform = _hintObjects[i].GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"{_hintObjects[i].name} does not have a RectTransform component!");
                continue;
            }

            _rectTransforms.Add(rectTransform);
            _onScreenPositions.Add(rectTransform.anchoredPosition);
            _offScreenPositions.Add(new Vector2(-_onScreenPositions[i].x, _onScreenPositions[i].y));
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < _hintObjects.Count; i++)
        {
            _rectTransforms[i].anchoredPosition = _offScreenPositions[i];
            _rectTransforms[i].DOAnchorPos(_onScreenPositions[i], 0.5f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
        }
    }

    public void CloseNavigator()
    {
        for (int i = 0; i < _hintObjects.Count; i++)
        {
            _rectTransforms[i].DOAnchorPos(_offScreenPositions[i], 0.5f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
                .OnComplete(OnComplete);
        }
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);
    }
}
