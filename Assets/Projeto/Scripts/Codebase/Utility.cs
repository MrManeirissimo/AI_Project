using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA_Project
{
    namespace Utility
    {
        namespace Enumerators
        {
            public enum TileTypes
            {
                Default,
                Straight,
                Curve,
                Cross
            }

            public enum BitTypes
            {
                None = 0,
                Stright = 1,
                Curve = 2,
                Cross = 4
            }

            public enum Directions
            {
                Up,
                Right,
                Down,
                Left,
            }
        }

        namespace Classes
        {
            using Enumerators;
            using Types;

            [System.Serializable]
            public struct IRange
            {
                public long min, max;

                public IRange(long min, long max)
                {
                    this.min = min;
                    this.max = max;
                }
            }

            [System.Serializable]
            public class PairAgentIRange
            {
                List<List<TileData>> map;
                IRange range;

                public IRange Range
                {
                    get { return (range); }
                }

                public PairAgentIRange(List<List<TileData>> map, long min, long max)
                {
                    this.map = map;
                    this.range = new IRange(min, max);
                }
                public PairAgentIRange(List<List<TileData>> map, IRange range) : this(map, range.min, range.max)
                {

                }
            }

            [System.Serializable]
            public class TileData
            {
                public Directions direction;
                public TileTypes type;

                public TileData(Directions direction, TileTypes type)
                {
                    this.direction = direction;
                    this.type = type;
                }
            }

            [System.Serializable]
            public class DirectionCoord
            {
                private Vector2 _coord;
                private Directions _dir;

                public Directions Dir
                {
                    get { return (_dir); }
                }
                public int X
                {
                    get { return ((int)_coord.x); }
                }
                public int Y
                {
                    get { return ((int)_coord.y); }
                }
                public static Vector2 Coordinates(Directions dir)
                {
                    switch (dir)
                    {
                        case Directions.Up:
                            return (Vector2.up);
                        case Directions.Right:
                            return (Vector2.right);
                        case Directions.Left:
                            return (Vector2.left);
                        case Directions.Down:
                            return (Vector2.down);

                        default:
                            return (Vector2.zero);
                    }
                }

                public DirectionCoord(Directions dir)
                {
                    _coord = Coordinates(dir);
                    _dir = dir;
                }
            }

            [System.Serializable]
            public class InterestMatrix
            {
                #region Variables
                public int[,] _self;
                #endregion

                #region Static member functions
                /// <summary>
                /// Rotates an InterestMatrix in increments of 90 degrees
                /// </summary>
                /// <param name="matrix"></param>
                /// <param name="multiplier"></param>
                /// <returns></returns>
                public static InterestMatrix RotateMatrix(InterestMatrix matrix, int multiplier)
                {
                    double angle = (-90 * multiplier) / (180 / System.Math.PI);
                    double __cos = System.Math.Cos(angle);
                    double __sin = System.Math.Sin(angle);


                    int[,] __finalMatrix = new int[3, 3];
                    int __currentValue = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            __currentValue = matrix._self[i, j];

                            double x = (System.Math.Round(((i - 1) * __cos)) - System.Math.Round(((j - 1) * __sin)));
                            double y = (System.Math.Round(((i - 1) * __sin)) + System.Math.Round(((j - 1) * __cos)));

                            x++;
                            y++;

                            __finalMatrix[(int)x, (int)y] = __currentValue;
                        }
                    }

                    return (new InterestMatrix(__finalMatrix));
                }
                #endregion

                #region Public member functions
                public bool ValidAtIndex(Directions dir)
                {
                    Vector2 __coord = DirectionCoord.Coordinates(dir);
                    return (_self[(int)__coord.x + 1, (int)__coord.y + 1] >= 1 ? true : false);
                }
                public bool ValidAtIndex(DirectionCoord coord)
                {
                    return (_self[coord.X + 1, coord.Y + 1] >= 1 ? true : false);
                }
                public bool ValidAtIndex(int row, int column)
                {
                    return (_self[row, column] >= 1 ? true : false);
                }
                #endregion

                #region Ctor's
                public InterestMatrix()
                {
                    _self = new int[3, 3];
                }
                public InterestMatrix(TileTypes type)
                {
                    if (type > 0)
                    {
                        switch (type)
                        {
                            case TileTypes.Straight:
                                {
                                    _self = new int[3, 3] { { 0, 1, 0 }, { 0, 0, 0 }, { 0, 1, 0 } };
                                    break;
                                }

                            case TileTypes.Curve:
                                {
                                    _self = new int[3, 3] { { 0, 1, 0 }, { 1, 0, 0 }, { 0, 0, 0 } };
                                    break;
                                }

                            case TileTypes.Cross:
                                {
                                    _self = new int[3, 3] { { 0, 1, 0 }, { 1, 0, 1 }, { 0, 1, 0 } };
                                    break;
                                }
                        }
                    }
                    else
                    {
                        _self = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                    }
                }
                public InterestMatrix(int[,] matrix)
                {
                    this._self = matrix;
                }
                #endregion
            }

            [System.Serializable]
            public class BitMatrix : InterestMatrix
            {
                public BitMatrix()
                {
                    _self = new int[3, 3];
                }
                public BitMatrix(TileTypes type)
                {
                    if (type > 0)
                    {
                        switch (type)
                        {
                            case TileTypes.Straight:
                                {
                                    _self = new int[3, 3]   {
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve + (int)BitTypes.Cross, 0 },
                                                    {0, 0, 0 },
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve + (int)BitTypes.Cross, 0 }
                                                };
                                }
                                break;

                            case TileTypes.Curve:
                                {
                                    _self = new int[3, 3]   {
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve, 0 },
                                                    {0, 0, 0 },
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve, 0 }
                                                };
                                }
                                break;

                            case TileTypes.Cross:
                                {
                                    _self = new int[3, 3]   {
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve , 0 },
                                                    {(int)BitTypes.Stright + (int)BitTypes.Curve , 0, (int)BitTypes.Stright + (int)BitTypes.Curve  },
                                                    {0, (int)BitTypes.Stright + (int)BitTypes.Curve, 0 }
                                                };
                                }
                                break;
                        }
                    }
                    else
                    {
                        _self = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                    }
                }

                public static BitMatrix operator *(BitMatrix bitMatrix, InterestMatrix itMatrix)
                {
                    return (new BitMatrix());
                }
            }

            public class FuzzyLogger
            {
                private Fuzzy.FuzzyBehaviour _fBehaviour;

                public FuzzyLogger(Fuzzy.FuzzyBehaviour behaviour)
                {

                }
            }
        }

        namespace Types
        {
            using Classes;

            public delegate void VDelegate();
            public delegate void AgntAction(List<List<TileData>> agent);
            public delegate bool BooleanAssert(List<object> objList);
        }

        public static class Functions
        {
            public static void Shuffle<T>(this IList<T> list)
            {
                System.Random rng = new System.Random();
                int n = list.Count;

                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);

                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }
    }
}