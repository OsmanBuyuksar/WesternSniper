using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CamerManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera playerCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;
    public void EnableScopeCamera(bool isScopeEnabled)
    {
        aimCam.enabled = isScopeEnabled;
        playerCam.enabled = !isScopeEnabled;
    }

    internal CinemachineVirtualCamera getActiveCam()
    {
        return playerCam;
    }
}
