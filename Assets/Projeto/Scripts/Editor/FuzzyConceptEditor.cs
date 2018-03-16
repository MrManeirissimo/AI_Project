using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Fuzzy;

[CustomEditor(typeof(Fuzzy.Concept))]
public class FuzzyConceptEditor : Editor
{
    //References
    private Concept _self;
    private FunctionConfig _functionConfig;

    //Options
    private int _selectedPopup;
    private bool _showFunctions;
    private static float _defaultSpacing = 20;

    //Containers
    private string[] _functionNames;
    private int[] _functionIndexes;

    //Events
    public event System.Action FunctionAdded;
    public event System.Action FunctionRemoved;

    //Utility
    private void ResizeFunctionStringArray()
    {
        if (_self.Functions == null)
            return;

        int __size = _self.Functions.Count;
        _functionNames = new string[__size];
        _functionIndexes = new int[__size];

        for (int i = 0; i < __size; i++)
        {
            _functionNames[i] = _self.Functions[i].Name;
            _functionIndexes[i] = i;
        }

        _selectedPopup = -1;
    }

    //Core display fucntion
    private void DrawFunctions()
    {
        GUILayout.BeginVertical();
        {
            //Dropdow list
            _showFunctions = EditorGUILayout.Foldout(_showFunctions, "Functions");

            if (_showFunctions)
            {
                if (_self.Functions != null && _self.Functions.Count > 0)
                {
                    for (int i = 0; i < _self.Functions.Count; i++)
                    {
                        GUILayout.BeginVertical();
                        {
                            //Display function name and name field
                            EditorCommons.FuzzyFuncName(_self.Functions[i]);

                            //Display function body (graph)
                            EditorCommons.FuzzyFuncBody(_self.Functions[i]);

                            //Display function begin coordinates
                            EditorCommons.FuzzyBeginCoord(_self.Functions[i]);

                            //Display function end coordinates
                            EditorCommons.FuzzyEndCoord(_self.Functions[i]);
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(_defaultSpacing);
                    }
                }
            }
        }
        GUILayout.EndVertical();
    }
    private void DisplayInspectorHeader()
    {
        //Name display box
        GUILayout.BeginVertical("box");
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(150);
                GUILayout.Label(_self.name);
                GUILayout.Space(150);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndVertical();

        //Name input field
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Concept name: ", EditorStyles.boldLabel);
            _self.name = GUILayout.TextField(_self.name, GUILayout.Width(250));
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    }
    private void AddFunctionFunctionality()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(_defaultSpacing);
            if (GUILayout.Button("Add function", GUILayout.Width(150)))
            {
                if (_functionConfig == null)
                {
                    _self.AddFunction();
                }
                else
                {
                    _self.AddFunction(_functionConfig);
                    _functionConfig = null;
                }

                _showFunctions = true;

                if (FunctionAdded != null)
                    FunctionAdded();
            }
            _functionConfig = (FunctionConfig) EditorGUILayout.ObjectField(_functionConfig, typeof(FunctionConfig), true);
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    }
    private void RemoveFunctionFunctionality()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(_defaultSpacing);
            {
                if (GUILayout.Button("Remove function", GUILayout.Width(150)))
                {
                    if(_selectedPopup < 0)
                    {
                        Debug.LogError("@Editor exception - Could dropdown value not assigned");
                        return;
                    }

                    _self.RemoveFunction(_functionNames[_selectedPopup]);

                    if (FunctionRemoved != null)
                        FunctionRemoved();
                }

                _selectedPopup = EditorGUILayout.IntPopup(_selectedPopup, _functionNames, _functionIndexes);
            }
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    }
    
    private void OnEnable()
    {
        _self = (Concept)target;
        _functionNames = new string[0];
        _functionIndexes = new int[0];

        ResizeFunctionStringArray();

        FunctionAdded += ResizeFunctionStringArray;
        FunctionRemoved += ResizeFunctionStringArray;
    }
    public override void OnInspectorGUI()
    {
        //Concept name display
        DisplayInspectorHeader();

        //Draw function graphs
        DrawFunctions();

        //Add function button
        AddFunctionFunctionality();

        //Remove function button
        RemoveFunctionFunctionality();

        //Updates graphs
        _self.UpdateAllFunctionValues();
    }
}