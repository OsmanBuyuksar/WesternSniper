using UnityEngine;

public class TouchInput : MonoBehaviour
{
   [HideInInspector] public float horizontal;

    private float startTouch;
    private float swipeDelta;

    private float velocityX = 0f;

    private void Update()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition.x;
        }
        if (Input.GetMouseButton(0))
        {
            swipeDelta = Input.mousePosition.x - startTouch;
            startTouch = Input.mousePosition.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            swipeDelta = 0f;
        }
        horizontal = Mathf.SmoothDamp(horizontal, swipeDelta, ref velocityX, 0f);
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
}
