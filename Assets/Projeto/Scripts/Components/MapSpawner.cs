using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapSpawner : MonoBehaviour
{
    [SerializeField] public GameObject[] _prefabs;
    [SerializeField] private MapConfiguration _mConfig;

    [SerializeField] private List<GameObject> _tiles;
    
    [ContextMenu("Build")]
    public void Build()
    {
        if (_mConfig)
        {
            GameObject __root = new GameObject("Instance_Map");
            __root.transform.position = _mConfig.Positions[0];


            MapConstruction Map = __root.AddComponent<MapConstruction>();
            for (int i = 0; i < _mConfig.Size; i++)
            {
                GameObject spawn = _prefabs[_mConfig.Tiles[i] - 1];
                spawn = (GameObject)Instantiate(spawn, _mConfig._positions[i], _mConfig._rotations[i], __root.transform);

                if (_mConfig.Tiles[i] == (int)MapMakerIII.Blocks.Curve)
                {
                    Map.AddCurve(spawn);
                }

                _tiles.Add(spawn);
            }
        }
    }

    private void BuildMap()
    {
        
    }
}
