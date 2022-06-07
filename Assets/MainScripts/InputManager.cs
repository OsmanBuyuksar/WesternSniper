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
    [SerializeField] private PlayerAimer player;
    [SerializeField] private CinemachineImpulseSource impulse;

    bool scope = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            scope = CheckIfInside();
            EnableScope(scope);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (scope)
            {
                impulse.GenerateImpulse();
                player.Fire();
            }
            scope = false;
            EnableScope(scope);
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
