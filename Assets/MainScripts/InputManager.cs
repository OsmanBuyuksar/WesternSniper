using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class InputManager : MonoBehaviour
{
    [SerializeField] TouchInput input;
    [SerializeField] RectTransform touchToAimImg;
    [SerializeField] private CamerManager camManage;
    [SerializeField] private UXManager uxManage;
    [SerializeField] private CinemachineImpulseSource impulse;

    private bool swiping = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            swiping = true;
            EnableScope(CheckIfInside()  || swiping);
        }
        else if (Input.GetMouseButtonUp(0))
            impulse.GenerateImpulse(camManage.getActiveCam().transform.forward);
        else 
        {
            
            EnableScope(false);
            swiping = false; 
        }
        
    }
    private bool CheckIfInside()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(touchToAimImg, Input.mousePosition);
    }

    private void EnableScope(bool isScopeEnabled)
    {
        uxManage.EnableScopeCanvas(isScopeEnabled);
        camManage.EnableScopeCamera(isScopeEnabled);
    }
}
