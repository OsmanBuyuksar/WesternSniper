using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UXManager : MonoBehaviour
{
    [SerializeField] private GameObject aimScopeCanvas;
    [SerializeField] private GameObject gamePlayCanvas;
    
    public void EnableScopeCanvas(bool isScopeEnabled)
    {
        aimScopeCanvas.SetActive(isScopeEnabled);
        gamePlayCanvas.SetActive(!isScopeEnabled);
    }
}
