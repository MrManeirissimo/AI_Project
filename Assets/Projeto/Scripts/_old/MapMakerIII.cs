using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMakerIII : MonoBehaviour
{

    public List<int> intList;
    public List<int> Ints
    {
        get
        {
            if (intList == null)
                intList = new List<int>();

            return intList;
        }
    }

    public List<GameObject> tiles;
    public List<GameObject> Tiles
    {
        get
        {
            if (tiles == null)
                tiles = new List<GameObject>();

            return (tiles);
        }
    }


    #region Nested enums
    public enum Blocks
    {
        Null = 0,
        Road = 1,
        Curve = 2,
        Intersection = 3
    }                                                           //Enum used for block tag-type comparison
    public enum Directions
    {
        Up, Down, Left, Right
    }                                                       //Enum used for definig building direction
    #endregion

    #region Nested calsses
    public class PGObj_Vector                                                      //Pair class used to easily acces the matrix postion in the world and its value
    {
        public GameObject Value;                                                           //Block type
        public Vector3 Pos;                                                         //Matrix slot world position

        public PGObj_Vector(GameObject value, Vector2 position)                           //ctor
        {
            Value = value;
            Pos = new Vector3(position.x, 0, position.y);
        }                       
    }          
    
    [System.Serializable]
    public class Indexer                                                            //Class used to easily navigate the map matrix
    {
        public int x;                                                               //X coord representation in the MapMatrix
        public int y;                                                               //Y coord representation in the MapMatrix

        #region Contructors
        public Indexer()
        {
            x = y = 0;
        }
        public Indexer(Vector2 vec)
        {
            x = (int)vec.x;
            y = (int)vec.y;
        }
        public Indexer(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        #endregion

        public static Indexer operator + (Indexer ind, Vector2 vec)                 //Indexer (+) operator overload 
        {
            ind.x = (int)vec.x;                                                     //When summing a vector with an indexer vector's x value is atributed to indexer's x field
            ind.y = (int)vec.y;                                                     //When summing a vector with an indexer vector's y value is atributed to indexer's y field

            return (ind);
        }             

        public void Log()                                                           //Public access debug poccess (Debugs x and y values) 
        {
            Debug.LogWarning("Indexer pos|x|: " + x + " : |y|: " + y);
        }
    }
    #endregion

    public int _maxBlocksPerRoad = 5;
    public int _numberOfRoads = 10;
    public static int mapSize = 60;
    
    public PGObj_Vector[,] Map = new PGObj_Vector[mapSize, mapSize];
    private Indexer currentSpot = new Indexer(0, 0);

    private int blockSize = 10;
    private bool buildCurve = false;
    private GameObject[] _prefabs;

    private void Initialize()
    {
        //Initializes the each slot of the Matrix map with a pair of Value-Position
        //For each loop itteration the position offset of the grid is placed as the position inside the pair
        //And the pair value is set to 0 (or empty road)
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Map[i, j] = new PGObj_Vector(null, new Vector2((blockSize * (i + 1)) +5, (blockSize * (j + 1)) +5));
            }
        }

        //Alternative block for dealing with the need to pass the prefab array to most of the functions as parameters
        //Here we're loading and storing the prefabs on a class field variable for later use
        {
            Object[] prefabs = LoadPrefabs();

            _prefabs = new GameObject[prefabs.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                _prefabs[i] = (GameObject)prefabs[i];
            }
        }
    }
    private void DebugMap()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                print(Map[i, j].Pos);
            }
        }
    }

    private Object[] LoadPrefabs()
    {
        return (Resources.LoadAll("Prefabs"));
    }
    private Vector2 FindPos()
    {
        return (new Vector2(Random.Range(0, mapSize), Random.Range(0, mapSize)));
    }
    private Directions GetRandomDirection()
    {
        int enumSize = System.Enum.GetNames(typeof(Directions)).Length;
        return ((Directions)Random.Range(0, enumSize));
    }
    private Directions GetRandomCurveDirection(Directions __previousDir)
    {
        int __directionIndex = -1;

        __directionIndex = (__previousDir == Directions.Up || __previousDir == Directions.Down) ?
            Random.Range((int)Directions.Left, (int)Directions.Right + 1) :
            Random.Range((int)Directions.Up, (int)Directions.Down + 1);

        return ((Directions)__directionIndex);
    }
    private int AssertDirection(Directions dir)
    {
        int __rValue = -1;
        switch(dir)
        {
            case Directions.Up:
                {
                    __rValue = (mapSize - 1) - currentSpot.y;
                    break;
                }

            case Directions.Down:
                {
                    __rValue = currentSpot.y;
                    break;
                }

            case Directions.Left:
                {
                    __rValue = currentSpot.x;
                    break;
                }

            case Directions.Right:
                {
                    __rValue = (mapSize - 1) - currentSpot.x;
                    break;
                }

            default:
                {
                    __rValue = -1;
                    break;
                }

        }//Switch-case


        return (__rValue);
    }
    private bool IsRoadBuildable(int __roadSize, Directions __dir)
    {
        bool __returnValue = false;
        if(__roadSize > 0)
        {
            int intersectingBlocks = 0;
            switch (__dir)
            {
                case Directions.Up:
                    {
                        for (int i = 1; i <= __roadSize; i++)
                        {
                            if (Map[currentSpot.x, currentSpot.y + i].Value)
                            {
                                intersectingBlocks++;
                                break;
                            }
                        }
                        break;
                    }

                case Directions.Down:
                    {
                        for (int i = 1; i <= __roadSize; i++)
                        {
                            if (Map[currentSpot.x, currentSpot.y - i].Value)
                            {
                                intersectingBlocks++;
                                break;
                            }
                        }
                        break;
                    }

                case Directions.Right:
                    {
                        for (int i = 1; i <= __roadSize; i++)
                        {
                            if (Map[currentSpot.x + i, currentSpot.y].Value)
                            {
                                intersectingBlocks++;
                                break;
                            }
                        }
                        break;
                    }

                case Directions.Left:
                    {
                        for (int i = 1; i <= __roadSize; i++)
                        {
                            if (Map[currentSpot.x - i, currentSpot.y].Value)
                            {
                                intersectingBlocks++;
                                break;
                            }
                        }
                        break;
                    }
            }

            if (intersectingBlocks <= 0)
                __returnValue = true;

        } //if(roadsize < 0)

        return (__returnValue);
    }

    private GameObject InstantiateBlock(Object[] prefabs, Blocks type)
    {
        Object asset = null;
        foreach (var prefab in prefabs)
        {
            if ((prefab as GameObject).tag == type.ToString())
            {
                asset = prefab;
                break;
            }
        }

        GameObject __instance = Instantiate(asset, Map[currentSpot.x, currentSpot.y].Pos, Quaternion.identity)
            as GameObject;

        Map[currentSpot.x, currentSpot.y].Value = __instance;

        return (__instance);
    }
    private GameObject InstantiateBlock(Object[] prefabs, Blocks type, Directions dir)
    {
        Object asset = null;
        foreach (var prefab in prefabs)
        {
            if((prefab as GameObject).tag == type.ToString())
            {
                asset = prefab;
                break;
            }
        }

        if(asset)
        {
            //Moves the indexer relative to the direction given
            switch (dir)
            {
                case Directions.Up:
                    {
                        currentSpot.y++;
                        break;
                    }

                case Directions.Down:
                    {
                        currentSpot.y--;
                        break;
                    }

                case Directions.Left:
                    {
                        currentSpot.x--;
                        break;
                    }

                case Directions.Right:
                    {
                        currentSpot.x++;
                        break;
                    }

            }//switch

            GameObject __instance = Instantiate(asset, Map[currentSpot.x, currentSpot.y].Pos, Quaternion.identity)
                as GameObject;

            Map[currentSpot.x, currentSpot.y].Value = __instance;

            return (__instance);
        }

        return (null);
    }
    private void BuildRoad(int roadSize, Directions dir)
    {
        for (int i = 0; i < roadSize; i++)
        {
            if (dir == Directions.Right || dir == Directions.Left)
            {
                GameObject a = InstantiateBlock(_prefabs, Blocks.Road, dir);
                a.transform.Rotate(new Vector3(0, 90, 0));

                Ints.Add((int)Blocks.Road);
                Tiles.Add(a);
            }
                
            else
            {
                GameObject a  = InstantiateBlock(_prefabs, Blocks.Road, dir);
                Ints.Add((int)Blocks.Road);
                Tiles.Add(a);
            }
                
        }

        buildCurve = true;
    }
    private void BuildCurve(int __roadSize, Directions __from, Directions __to)
    {
        Quaternion __newRotation = Quaternion.identity;
        switch (__from)
        {
            case Directions.Up:
                {
                    currentSpot.y++;

                    __newRotation = __to == Directions.Right ?
                        Quaternion.Euler(new Vector3(0, 180, 0)) :
                        Quaternion.Euler(new Vector3(0, -90, 0));

                    break;
                }

            case Directions.Down:
                {
                    currentSpot.y--;

                    if (__to == Directions.Right)
                        __newRotation = Quaternion.Euler(new Vector3(0, 90, 0));

                    break;
                }

            case Directions.Left:
                {
                    currentSpot.x--;

                    __newRotation = __to == Directions.Down ?
                        Quaternion.Euler(new Vector3(0,180,0)) :
                        Quaternion.Euler(new Vector3(0, 90, 0));

                    break;
                }

            case Directions.Right:
                {
                    currentSpot.x++;

                    if (__to == Directions.Down)
                        __newRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                    break;
                }
        }

        if (!Map[currentSpot.x, currentSpot.y].Value)
        {
            GameObject a = InstantiateBlock(_prefabs, Blocks.Curve);
            a.transform.rotation = __newRotation;

            Tiles.Add(a);
            Ints.Add((int)Blocks.Curve);
        }
        else
        {
            int index = 0;
            if(Tiles.Contains(Map[currentSpot.x, currentSpot.y].Value))
            {
                for (int i = 0; i < Tiles.Count; i++)
                {
                    if(Tiles[i] == Map[currentSpot.x, currentSpot.y].Value)
                    {
                        index = i;
                    }
                }
            }

            Ints.RemoveAt(index);
            Tiles.RemoveAt(index);

            Destroy(Map[currentSpot.x, currentSpot.y].Value);
            GameObject a = InstantiateBlock(_prefabs, Blocks.Intersection);

            Tiles.Insert(index, a);
            Ints.Insert(index, (int)Blocks.Intersection);
        }
            

        buildCurve = false;
    }

    private void GenerateMap(Object[] prefabs)
    {
        int __roadCounter = 0;
        currentSpot = currentSpot + FindPos();          //Generates a random starting point for the circuit
                                                        //Using overloaded operator + to easily assing a new position to the Indexer object

        currentSpot.Log();                              //Prints the chosen spot

        GameObject __root = InstantiateBlock(prefabs, Blocks.Road); __root.name = "Root";
        Directions __previousDirection = GetRandomDirection();

        while (__roadCounter < _numberOfRoads)
        {
            int __maxRoadSize = 0;
            bool __roadBuilt = false;
            
            do
            {
                Directions __dir;

                if (buildCurve) { __dir = GetRandomCurveDirection(__previousDirection); }
                else { __dir = GetRandomDirection(); }

                __maxRoadSize = AssertDirection(__dir);

                if (__maxRoadSize > 0)
                {
                    //Corrects ROOT orientation
                    if (__root && __root.name == "Root")
                    {
                        if (__dir == Directions.Left || __dir == Directions.Right)
                        {
                            if (Map[currentSpot.x, currentSpot.y].Value.tag == Blocks.Road.ToString())
                                Map[currentSpot.x, currentSpot.y].Value.transform.Rotate(new Vector3(0, 90, 0));
                        }
                    }

                    if (buildCurve)
                    {
                        BuildCurve(0, __previousDirection, __dir);
                    }

                    __maxRoadSize = (__maxRoadSize > _maxBlocksPerRoad) ? _maxBlocksPerRoad : __maxRoadSize;

                    int __roadSize = Random.Range(1, __maxRoadSize);
                    if (IsRoadBuildable(__roadSize, __dir))
                    {
                        BuildRoad(__roadSize, __dir);
                        __previousDirection = __dir;

                        __roadBuilt = true;
                    }
                }

            } while (!__roadBuilt);
            

            __root = Map[currentSpot.x, currentSpot.y].Value;
            __roadCounter++;
        }
    }
    private Transform SelectRandomChilldPivot(GameObject prefab)
    {
        return (prefab.transform.GetChild(Random.Range(0, prefab.transform.childCount)));
    }

    #region Unity messages
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Gizmos.DrawWireCube(new Vector3(((blockSize * (i + 1)) + 5), 0, ((blockSize * (j + 1)) +5)), new Vector3(blockSize, 1, blockSize));
            }
        }

        Gizmos.color = new Color(0, 0.8f, 0.8f, 0.2f);
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Gizmos.DrawCube(new Vector3(((blockSize * (i + 1)) + 5), 0, ((blockSize * (j + 1)) + 5)), new Vector3(blockSize, 1, blockSize));
            }
        }
    }
    private void Start()
    {
        Initialize();
        GenerateMap(LoadPrefabs());
        
        //MapConfiguration __config = AssetCreator.CreateMapConfigurationAsset();

        //if (!__config)
        //    return;

        //for (int i = 0; i < Tiles.Count; i++)
        //{
        //    __config.Tiles.Add(intList[i]);
        //    __config.Positions.Add(tiles[i].transform.position);
        //    __config.Rotations.Add(tiles[i].transform.rotation);
        //}

    }
    private void Update()
    {
        
    }
    #endregion
}