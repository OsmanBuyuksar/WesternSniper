using UnityEngine;

public class TouchInput : MonoBehaviour
{
   [HideInInspector] public float horizontal;
   [HideInInspector] public float vertical;

    private Vector3 startTouch;
    private Vector3 swipeDelta;

    private float velocityX = 0f;
    private float velocityY = 0f;

    private void Update()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            swipeDelta = Input.mousePosition - startTouch;
            startTouch = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            swipeDelta = Vector3.zero;
        }
        horizontal = Mathf.SmoothDamp(horizontal, swipeDelta.x, ref velocityX, 0f);
        vertical = Mathf.SmoothDamp(vertical, swipeDelta.y, ref velocityY, 0f);
#else

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startTouch = touch.position.x;
            }
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                swipeDelta = touch.position.x - startTouch;
                startTouch = touch.position.x;
            }
            if (touch.phase == TouchPhase.Ended)
            {
                swipeDelta = 0f;
            }
        }
        horizontal = Mathf.SmoothDamp(horizontal, swipeDelta, ref velocityX, 0f);


#endif


    }

    public Vector3 GetSwipeVector()
    {
        return swipeDelta;
    }
}
