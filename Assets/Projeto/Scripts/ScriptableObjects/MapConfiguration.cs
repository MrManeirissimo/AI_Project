using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map configuration")]
public class MapConfiguration : ScriptableObject
{
    public List<int> _objects;
    public List<Vector3> _positions;
    public List<Quaternion> _rotations;

    public List<int> Tiles
    {
        get
        {
            if (_objects == null)
                _objects = new List<int>();

            return _objects;
        }
    }
    public List<Vector3> Positions
    {
        get
        {
            if (_positions == null)
                _positions = new List<Vector3>();

            return _positions;
        }
    }
    public List<Quaternion> Rotations
    {
        get
        {
            if (_rotations == null)
                _rotations = new List<Quaternion>();

            return _rotations;
        }
    }
    public int Size
    {
        get { return (_objects.Count); }
    }
}
