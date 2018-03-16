using MindTrick;
using System.Collections.Generic;
using UnityEngine;

using IA_Project.Utility.Types;
using IA_Project.Utility.Classes;
using IA_Project.Utility.Enumerators;

public class BotMap : MonoBehaviour
{
    #region Nested classes
    [System.Serializable] public class Block
    {
        [SerializeField] private GameObject _self;
        [SerializeField] private Vector2 _pos;

        public Block()
        {
            _pos = Vector2.zero;
            _self = null;
        }
        public Block(Vector2 __newPos)
        {
            _self = null;
            _pos = __newPos;
        }
        public Block(GameObject _object, Vector2 _newPos)
        {
            _pos = _newPos;
            _self = _object;
        }

        public GameObject Object
        {
            get { return (_self); }
            set { _self = value; }
        }
        public Vector2 Position
        {
            get { return (_pos); }
        }
        public Vector3 WorldPosition
        {
            get { return (new Vector3(_pos.x, 0, _pos.y)); }
        }
    }
    #endregion

    #region Events
    public event System.Action OnFinishedDrawing;
    public event AgntAction OnDrawAgent;
    #endregion

    [SerializeField] private Block[,] WorldMap;
    [SerializeField] private const int _blockSize = 10;

    [Header("Values")]
    [SerializeField] private int _mapSize = 10;
    [SerializeField] private float _displayInterval;

    [Header("Options")]
    [SerializeField] private bool _displayFittestOnly = false;

    [Header("States")]
    [SerializeField] private bool _mapFilled = false;

    [Header("Timer")]
    [SerializeField] private Timer _displayTimer;

    Queue<List<List<TileData>>> _drawQueue;

    private Dictionary<TileTypes, GameObject> _buildingBlocks = new Dictionary<TileTypes, GameObject>();

    #region Public Properties

    public bool Filled
    {
        get { return (_mapFilled); }
    }
    public int MapSize
    {
        get { return (_mapSize); }
        set { _mapSize = value; }
    }
    public bool DisplayFittestOnly
    {
        get { return (_displayFittestOnly); }
        set { _displayFittestOnly = value; }
    }
    public float DisplayInterval
    {
        get { return (_displayInterval); }
        set { _displayInterval = value; }
    }

    #endregion

    #region Public draw queue methods

    public void AddToDrawQueue(List<List<TileData>> mapGenerated)
    {
        _drawQueue.Enqueue(mapGenerated);
    }
    public void AddToDrawQueue(List<List<List<TileData>>> mapList)
    {
        foreach (var map in mapList)
        {
            _drawQueue.Enqueue(map);
        }
    }
    public void ClearDrawQueue()
    {
        _drawQueue.Clear();
    }
    public void DrawFromQueue()
    {
        if (_drawQueue.Count > 0)
        {
            List<List<TileData>> __currentMap = _drawQueue.Dequeue();
            DisplayCurrentMap(__currentMap);

            if (OnDrawAgent != null)
                OnDrawAgent(__currentMap);
        }
    }
    public void BeginDisplayLoop()
    {
        float __interval = _displayInterval * 2;

        _displayTimer = new Timer(__interval, true, DrawFromQueue);

        ModuleManager.Play(_displayTimer);
    }
    public void SetTimerState(bool state)
    {
        if (state) _displayTimer.Play();
        else _displayTimer.Pause();
    }

    #endregion


    /// <summary>
    /// Initializes the Block Map[,] with the accurate world positions
    /// </summary>
    public void ConstructMap()
    {
        WorldMap = new Block[_mapSize, _mapSize];
        for (int i = 0; i < _mapSize; i++)
        {
            for (int j = 0; j < _mapSize; j++)
            {
                WorldMap[i, j] = new Block(new Vector2(((_blockSize * (i + 1)) + 5), ((_blockSize * (j + 1)) + 5)));
            }
        }
    }

    /// <summary>
    /// Uses an Agent from wich to draw the individual tiles
    /// <param name="agnt"> Descricao do parametro </param>
    /// </summary>
    public void DisplayCurrentMap(List<List<TileData>> agnt)
    {
        if (_mapFilled)
            ClearMap();

        for (int i = 0; i < agnt.Count; i++)
        {
            for (int j = 0; j < agnt[i].Count; j++)
            {
                InstantiateBlock(_buildingBlocks[agnt[i][j].type], new Vector2(i, j), agnt[i][j].direction);
            }
        }

        _mapFilled = true;
    }

    

    /// <summary>
    /// Calls the pool manager to despawn all blocks inside Block Map[,]
    /// </summary>
    public void ClearMap()
    {
        for (int i = 0; i < _mapSize; i++)
        {
            for (int j = 0; j < _mapSize; j++)
            {
                PoolManager.Despawn(WorldMap[i, j].Object);
            }
        }
    }

    public List<List<TileData>> GenerateMapData()
    {
        List<List<TileData>> __list = new List<List<TileData>>();

        
        for (int i = 0; i < _mapSize; i++)
        {
            List<TileData> __row = new List<TileData>();
            for (int j = 0; j < _mapSize; j++)
            {
                __row.Add(GetRandomTile());
            }

            __list.Add(__row);
        }

        return (__list);
    }
    public TileData GetRandomTile()
    {
        int __tileCount = System.Enum.GetNames(typeof(TileTypes)).Length;
        int __directionsCount = System.Enum.GetNames(typeof(Directions)).Length;


        Directions __direction = (Directions)Random.Range(0, __directionsCount);
        TileTypes __type = (TileTypes)UnityEngine.Random.Range(0, __tileCount);
        TileData __data = new TileData(__direction, __type);

        return (__data);
    }

    private void InstantiateBlock(GameObject __block, Vector2 index, Quaternion __rotation)
    {
        Vector2 __worldPos = WorldMap[(int)index.x, (int)index.y].WorldPosition;
        WorldMap[(int)index.x, (int)index.y].Object = PoolManager.Spawn(__block, WorldMap[(int)index.x, (int)index.y].WorldPosition, __rotation);
    }
    private void InstantiateBlock(GameObject __block, Vector2 index, Directions direction)
    {
        Vector2 __worldPos = WorldMap[(int)index.x, (int)index.y].WorldPosition;

        Quaternion rotation = Quaternion.Euler(0, 90 * (int)direction, 0);
        WorldMap[(int)index.x, (int)index.y].Object = PoolManager.Spawn(__block, WorldMap[(int)index.x, (int)index.y].WorldPosition, rotation);
    }
    private void LoadBlocks()
	{
        for (int i = 0; i < System.Enum.GetNames(typeof (TileTypes)).Length; i++)
        {
            _buildingBlocks.Add((TileTypes)i, ResourcesManager.LoadTile((TileTypes)i));
        }
	}

    private void DrawGrid()
    {
        Gizmos.color = Color.grey;

        for (int i = 0; i < _mapSize; i++)
        {
            for (int j = 0; j < _mapSize; j++)
            {
                Gizmos.DrawWireCube(new Vector3(((_blockSize * (i + 1)) + 5), 0, ((_blockSize * (j + 1)) + 5)), new Vector3(_blockSize, 1, _blockSize));
            }
        }

        Gizmos.color = new Color(0.8f, 0.0f, 0.0f, 0.2f);
        for (int i = 0; i < _mapSize; i++)
        {
            for (int j = 0; j < _mapSize; j++)
            {
                Gizmos.DrawCube(new Vector3(((_blockSize * (i + 1)) + 5), 0, ((_blockSize * (j + 1)) + 5)), new Vector3(_blockSize, 1, _blockSize));
            }
        }
    }

    #region Unity messages

    private void OnDrawGizmos()
    {
        DrawGrid();
    }
    private void Awake()
    {
        _drawQueue = new Queue<List<List<TileData>>>();

        LoadBlocks();
    }

    #endregion  
}