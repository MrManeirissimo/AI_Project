//using System.Collections;
//using System.Collections.Generic;

//using UnityEngine;
//using UnityEditor;

//public static class AssetCreator
//{
//    public static MapConfiguration CreateMapConfigurationAsset()
//    {
//        MapConfiguration mapConfiguration = ScriptableObject.CreateInstance<MapConfiguration>();

//        string __assetPath = "Assets/Projeto/MapConfiguration";
//        if (!System.IO.Directory.Exists(__assetPath))
//        {
//            System.IO.Directory.CreateDirectory(__assetPath);
//        }

//        __assetPath = AssetDatabase.GenerateUniqueAssetPath(__assetPath + "/" + "Map configuration.asset");

//        AssetDatabase.CreateAsset(mapConfiguration, __assetPath);
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//        return (mapConfiguration);
//    }
//}
