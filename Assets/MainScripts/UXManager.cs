using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UXManager : MonoBehaviour
{
    [SerializeField] private Canvas aimScope;
    [SerializeField] private Canvas gamePlayCanvas;
    
    public void EnableScopeCanvas(bool isScopeEnabled)
    {
        aimScope.enabled = isScopeEnabled;
        gamePlayCanvas.enabled = !isScopeEnabled;
    }
}
