using UnityEngine;
using UnityEngine.EventSystems;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// This Class handles Swiping Interaction on the attached Component
/// </summary>
public class SwipeHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private float _swipeThreshold = 0.2f;
    [SerializeField] private float _swipeEasing = 0.5f;
    
    private Vector3 _currentPosition;
    private int _totalPages;
    private int _currentPage;

    void Start()
    {
        _currentPosition = transform.position;
        _totalPages = transform.childCount;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaPosition = eventData.pressPosition.y - eventData.position.y;
        transform.position = _currentPosition - new Vector3(0, deltaPosition, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float swipedPercentage = (eventData.pressPosition.y - eventData.position.y) / Screen.height;
        MoveContentBySwipe(swipedPercentage);
    }

    /// <summary>
    /// Given swipe percentage is greater than threshold, moves the content to next page, or resets
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
    /// </summary>
    /// <param name="swipe"></param>
    private void ConfirmSwipe(float swipe)
    {
        if ( (_currentPage + 1 > _totalPages - 1 && swipe < 0) || (_currentPage - 1 < 0 && swipe > 0))
        {
            ResetContent();
            return;
        }
        
        Vector3 newPosition = _currentPosition;
        newPosition += new Vector3(0, swipe > 0 ? -Screen.height : Screen.height, 0);

        LerpPosition(transform.position, newPosition, _swipeEasing);
        _currentPosition = newPosition;

        _ = (swipe > 0) ? _currentPage-- : _currentPage++;
    }

    /// <summary>
    /// Resets content to the currentPosition
    /// </summary>
    private void ResetContent()
    {
        LerpPosition(transform.position, _currentPosition, _swipeEasing);
    }


    /// <summary>
    /// Lerps the Content Position to a target position smoothly over a given duration
    /// </summary>
    /// <param name="currentPosition">Starting position</param>
    /// <param name="targetPosition">Target position to reach at the end of lerp</param>
    /// <param name="duration">How long the lerping should take</param>
    private async void LerpPosition(Vector3 currentPosition, Vector3 targetPosition, float duration)
    {
        float time = 0;
        while (time <= 1.0f)
        {
            time += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(currentPosition, targetPosition, Mathf.SmoothStep(0, 1, time));
            await Task.Yield();
        }
    }
    
    
}
