using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CarMapManager : MonoBehaviour
{
    private static CarMapManager _instace;
    public static CarMapManager Instance
    {
        get
        {
            if (_instace == null)
                _instace = new CarMapManager();

            return _instace;
        }

        set
        {
            if (_instace == null)
                _instace = value;

            else
                Destroy(value);
        }
    }

    
    public Transform[] GetCurveArray
    {
        get
        {
            return (_instace.map.CurveTransformArray);
        }
    }
    public MapConstruction Map
    {
        get
        {
            return (map);
        }
    }

    [Header("Containers")]
    [SerializeField] private GameObject[] mapArray;

    [Header("References")]
    [SerializeField] private GameObject carRef;
    [SerializeField] private MapConstruction map;

    [ContextMenu("SetUpCarsapwn")]
    private void SetUpCarsapwn()
    {
        if (carRef)
        {
            carRef.transform.position = map.transform.position + Vector3.up ;
            carRef.transform.rotation = map.SpawnPoint.rotation * Quaternion.Euler(0, 180, 0);

            FindObjectOfType<CamFollow>().SetCamera();
        }
    }

    private void Awake()
    {
        Instance = this;
        if(!this.map.gameObject.activeSelf || this.map == null)
        {
            var __mapsOnScene = FindObjectsOfType<MapConstruction>();
            foreach (var __currentMap in __mapsOnScene)
            {
                if(__currentMap.gameObject.activeSelf)
                {
                    map = __currentMap;
                    break;
                }
            }
        }

        SetUpCarsapwn();
    }

    #region Buttons

    public void Reload(UnityEngine.UI.Dropdown option)
    {
        if(option.value < mapArray.Length)
        {
            MapConstruction component = mapArray[option.value].GetComponent<MapConstruction>();
            if(component)
            {
                Car __car = carRef.GetComponent<Car>();
                if(__car)
                {
                    Rigidbody __body = __car.GetComponent<Rigidbody>();
                    if(__body)
                    {
                        __body.velocity = Vector3.zero;
                    }

                    map.gameObject.SetActive(false);
                    component.gameObject.SetActive(true);

                    map = component;
                    SetUpCarsapwn();

                    __car.Initialize(GetCurveArray);
                }
            }
        }
    }

    #endregion
}
