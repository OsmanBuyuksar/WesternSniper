using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.Curvy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDecected : MonoBehaviour
{
    SplineController mainSpline = null;
    CurvySpline path = null;

    private void Awake()
    {
        mainSpline = GetComponentInParent<SplineController>();
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position + transform.forward*-1, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            path = hit.collider.GetComponentInParent<CurvySpline>();

            if (path != null)
            {
                mainSpline.Spline = path;
            }
        }
    }
}
