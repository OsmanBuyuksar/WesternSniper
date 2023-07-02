using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AutoGenerator : MonoBehaviour
{
    [Header("@PATH CONFIG")]
    [SerializeField] GameObject pathFirtObject = null;
    [SerializeField] private float startPoint = 0;
    [SerializeField] int pointCount = 8;
    [SerializeField] float distancePointsArea = 12;
    [SerializeField] int randomXValue;
    [SerializeField] int randomYValue;

    List<GameObject> pointsList = new List<GameObject>();

    [Header("@START & FINISH")]
    [SerializeField] GameObject startObject = null;
    [SerializeField] GameObject finishObject = null;

    private GameObject stObj = null;
    private GameObject finObj = null;

    public void Generate()
    {
        if (pointsList.Count == 0)
        {
            for (int i = 0; i < pointCount; i++)
            {
                var newPoints = Instantiate(pathFirtObject, new Vector3(Random.Range(0, randomXValue), Random.Range(0, randomYValue), startPoint), Quaternion.identity);
                newPoints.transform.SetParent(transform);
                pointsList.Add(newPoints);
                startPoint += distancePointsArea;

                pointsList[0].transform.localPosition = Vector3.zero;
                pointsList[0].transform.localEulerAngles = Vector3.zero;
            }

            stObj = Instantiate(startObject, new Vector3(0, -6.84f, -3.27f), Quaternion.identity);
            finObj = Instantiate(finishObject, new Vector3(pointsList[pointsList.Count - 1].transform.localPosition.x, pointsList[pointsList.Count - 1].transform.localPosition.y - 6.87f, pointsList[pointsList.Count - 1].transform.localPosition.z - -3.27f), Quaternion.identity);
            stObj.transform.SetParent(transform);
            finObj.transform.SetParent(transform);
        }

        else if (pointsList.Count > 0)
        {
            for (int i = 0; i < pointsList.Count; i++)
            {
                DestroyImmediate(pointsList[i]);
            }
            pointsList.Clear();
            startPoint = 0;

            DestroyImmediate(stObj);
            DestroyImmediate(finObj);
        }

    }
}