using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IA_Project.Utility.Enumerators;

public static class ResourcesManager
{
    #region Constant Fields

    private const string TileFormat = "Tiles/{0}";

    #endregion

    #region Public Methods

    public static GameObject LoadTile(TileTypes type)
    {
        return Resources.Load<GameObject>(string.Format(TileFormat, type));
    }

    #endregion
}
