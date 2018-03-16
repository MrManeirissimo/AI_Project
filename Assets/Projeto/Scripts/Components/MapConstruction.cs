using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapConstruction : MonoBehaviour
{
    private Queue<GameObject> _curves;
    private Vector3 _nextCurveDirection;

    public Vector3 SpawnPosition
    {
        get
        {
            if(transform.childCount > 0)
            {
                return transform.GetChild(0).position;
            }

            return Vector3.zero;
        }
    }
    public Transform SpawnPoint
    {
        get
        {
            return (transform.GetChild(0));
        }
    }
    public Vector3 DirectionOfCurve
    {
        get
        {
            return (_nextCurveDirection);
        }
    }
    public Transform[] CurveTransformArray
    {
        get
        {
            GameObject[] array =  Curves.ToArray();
            Transform[] returnValue = new Transform[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                returnValue[i] = array[i].transform;
            }

            return (returnValue);
        }
    }
    public Queue<GameObject> Curves
    {
        get
        {
            if (_curves == null)
                _curves = new Queue<GameObject>();

            return _curves;
        }
    }

    public void AddCurve(GameObject curve)
    {
        Curves.Enqueue(curve);
    }
    public GameObject GetNextCurve()
    {
        Curves.Dequeue();

        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).gameObject == Curves.Peek())
            {
                _nextCurveDirection = transform.GetChild(i).position - transform.GetChild(i).position;
            }
        }

        if (Curves.Count > 0)
            return (Curves.Peek());

        else
            return null;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Vector3 direction = transform.GetChild(i + 1).position - transform.GetChild(i).position;
            Vector3 finalPos = transform.GetChild(i).position + direction * 0.8f;
            Vector3 ang = Quaternion.AngleAxis(30, finalPos).eulerAngles;

            Gizmos.DrawLine(transform.GetChild(i).position + Vector3.up, finalPos + Vector3.up);
        }
    }
    public void Awake()
    {
        if(Curves.Count <= 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).tag == "Curve")
                {
                    Curves.Enqueue(transform.GetChild(i).gameObject);
                }
            }
        }
    }
}
