using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// This Class handles Swiping Interaction on the attached Component
/// </summary>
public class SwipeHandler : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField] private float _swipeThreshold = 0.2f;
    [SerializeField] private float _swipeEasing = 0.5f;

    #endregion

    #region private variables

    private float _currentPosition;
    private int _totalPages;
    private int _currentPage;

    private bool _dragging = false;
    private float _pointerDownPosition;
    private float _pointerUpPosition;

    private UIDocument _canvasDocument;
    private VisualElement _containerElement;

    #endregion

    #region Monobehaviour Functions

    private void Awake()
    {
        _canvasDocument = GetComponent<UIDocument>();
        _containerElement = _canvasDocument.rootVisualElement.Q<VisualElement>("Content");
    }

    private void OnEnable()
    {
        _containerElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _containerElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
        _containerElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    private void OnDisable()
    {
        _containerElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        _containerElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        _containerElement.UnregisterCallback<PointerDownEvent>(OnPointerDown);
    }
    
    void Start()
    {
        _currentPosition = _containerElement.style.top.value.value;
        _totalPages = _containerElement.childCount;
    }

    #endregion

    #region Pointer Functions

    private void OnPointerDown(PointerDownEvent evt)
    {
        _dragging = true;
        _pointerDownPosition = evt.position.y;
    }

    private void OnPointerMove(PointerMoveEvent eventData)
    {
        if (!_dragging) return;
        
        float deltaPosition = eventData.deltaPosition.y;
        
        _containerElement.style.top = _currentPosition + deltaPosition;
        _currentPosition = _containerElement.style.top.value.value;
    }

    private void OnPointerUp(PointerUpEvent eventData)
    {
        _pointerUpPosition = eventData.position.y;
        float swipedPercentage = (_pointerUpPosition - _pointerDownPosition) / Screen.height;
        MoveContentBySwipe(swipedPercentage);

        _dragging = false;
    }

    #endregion

    /// <summary>
    /// Given swipe value is greater than threshold, moves the content to next page, or resets
    /// </summary>
    /// <param name="swipe">Swipe Percentage</param>
    private void MoveContentBySwipe(float swipe)
    {
        if (Mathf.Abs(swipe) >= _swipeThreshold)
        {
            ConfirmSwipe(swipe);
        }
        else
        {
            ResetContent();
        }
    }

    /// <summary>
    /// Confirms swipe to move content to another page depending on swipe percentage it'd go to next or previous page
    /// Also Handles constraint for end of page lists
    /// </summary>
    /// <param name="swipe"></param>
    private void ConfirmSwipe(float swipe)
    {
        if ( (_currentPage + 1 > _totalPages - 1 && swipe < 0) || (_currentPage - 1 < 0 && swipe > 0))
        {
            ResetContent();
            return;
        }
        
        _ = (swipe > 0) ? _currentPage-- : _currentPage++;

        LerpPosition(_currentPosition, GetPagePosition(_currentPage), _swipeEasing);
    }

    /// <summary>
    /// Resets content to the currentPosition
    /// </summary>
    private void ResetContent()
    {
        LerpPosition(_currentPosition, GetPagePosition(_currentPage), _swipeEasing);
    }

    /// <summary>
    /// Gets the Position for the given page
    /// </summary>
    /// <param name="page">Page Number</param>
    /// <returns></returns>
    private float GetPagePosition(int page)
    {
        return -Screen.height * page;
    }


    /// <summary>
    /// Lerps the Content Position to a target position smoothly over a given duration
    /// </summary>
    /// <param name="currentPosition">Starting position</param>
    /// <param name="targetPosition">Target position to reach at the end of lerp</param>
    /// <param name="duration">How long the lerping should take</param>
    private async void LerpPosition(float currentPosition, float targetPosition, float duration)
    {
        float time = 0;
        while (time <= 1.0f)
        {
            time += Time.deltaTime / duration;
            _containerElement.style.top = Mathf.Lerp(currentPosition, targetPosition, Mathf.SmoothStep(0, 1, time));
            await Task.Yield();
        }

        _currentPosition = _containerElement.style.top.value.value;
    }
    
    
}
