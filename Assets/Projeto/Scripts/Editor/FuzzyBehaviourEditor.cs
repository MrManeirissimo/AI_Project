using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Fuzzy;

//[CustomEditor(typeof(Fuzzy.FuzzyBehaviour))]
public class FuzzyBehaviourEditor : Editor
{
    private new FuzzyBehaviour target;
    private static EditorConditionDisplay _conditionDisplayer;
    private static SerializedObject _objectReference;
    private static bool doOnlyOnce = false;

    //Auxiliar static containers
    private List<bool> _displayFunctions;
    private List<Color> _conditionColor;

    //Auxiliar
    private int _selectedConcept;
    private bool _showConcepts;
    private bool _displayConditions;

    private Concept CreateConceptInstance()
    {
        Concept __concept = CreateInstance<Concept>();

        string __assetPath = "Assets/Fuzzy_ScriptableObjects";
        if(!System.IO.Directory.Exists(__assetPath))
        {
            System.IO.Directory.CreateDirectory(__assetPath);
        }

        __assetPath = AssetDatabase.GenerateUniqueAssetPath(__assetPath + "/New " + typeof(Concept).ToString() + ".asset");

        AssetDatabase.CreateAsset(__concept, __assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = __concept;

        return (__concept);
    }

    private void DisplayConceptAndFunctions()
    {
        if (_showConcepts = EditorGUILayout.Foldout(_showConcepts, "Concept List"))
        {
            for (int index = 0; index < target.ConceptList.Count; index++)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        _displayFunctions[index] = EditorGUILayout.Foldout(_displayFunctions[index], target.ConceptList[index].name);

                        EditorGUILayout.ObjectField(target.ConceptList[index], typeof(Concept), false);

                        //Editable box with the concept value
                        target.SetConceptValue(index, EditorGUILayout.FloatField(target.ConceptList[index].Value, GUILayout.Width(50)));
                    }
                    GUILayout.EndHorizontal();

                    if (_displayFunctions[index])
                    {
                        foreach (Function func in target.ConceptList[index].Functions)
                        {
                            EditorCommons.FuzzyFuncName(func);
                            EditorCommons.FuzzyFuncBody(func);
                            EditorCommons.FuzzyBeginCoord(func);
                            EditorCommons.FuzzyEndCoord(func);

                            GUILayout.Space(EditorCommons._defaultSpacing);
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(EditorCommons._defaultSpacing);
        }
    }

    private void AddConceptFunctionality()
    {
        GUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Add concept"))
            {
                EditorGUIUtility.ShowObjectPicker<Concept>(new UnityEngine.Object(), false, string.Empty, (int)EditorCommons.WindowCommands.AddConcept);
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(this);
            }
        }
        GUILayout.EndHorizontal();
    }
    private void GenerateRandomConditionColor()
    {
        int difference = target.ConditionList.Count - ConditionColor.Count;
        for (int i = 0; i < difference; i++)
        {
            ConditionColor.Add(UnityEngine.Random.ColorHSV(0, 1, 0.2f, 0.3f, 0.7f, 1.0f, 0.8f, 1.0f));
        }
    }


    #region Accessors

    private EditorConditionDisplay ConditionDisplayer
    {
        get
        {
            if(!_conditionDisplayer)
                _conditionDisplayer = new EditorConditionDisplay(target);

            return (_conditionDisplayer);
        }
    }
    private List<Color> ConditionColor
    {
        get
        {
            if (_conditionColor == null)
                _conditionColor = new List<Color>();

            return (_conditionColor);
        }
    }
    private SerializedObject ObjectReference
    {
        get
        {
            if (_objectReference == null)
                _objectReference = new SerializedObject(base.target);

            return (_objectReference);
        }

    }

    #endregion

    #region Implementation methods

    /// <summary>
    /// Implementation and operations to remove concept from the class instance attached to this inspector
    /// </summary>
    private void RemoveConceptFromTargetList()
    {
        if (_selectedConcept >= target.ConceptList.Count || _selectedConcept < 0)
            throw new System.Exception("@Fuzzy Editor Exception - Index out of range");

        _conditionDisplayer.RemoveConceptName(_conditionDisplayer.GetConceptByIndex(_selectedConcept));

        if (target.ConditionList.Count <= 1)
        {
            //Clears the conditions because there is no available concepts
            target.RemoveCondition();
            _conditionDisplayer.RemoveCondition();

            //Removes the concept
            target.ConceptList.RemoveAt(_selectedConcept);
        }
        else
        {
            //Resets expressions with using the selected concept
            _conditionDisplayer.ResetExpressionsUsingConcept(_selectedConcept);

            //Removes the concept
            target.ConceptList.RemoveAt(_selectedConcept);
        }

        _selectedConcept = -1;
    }

    /// <summary>
    /// Method where the concepts and functions in each condition are called and displayed in the inspector
    /// </summary>
    /// <param name="conditionIndex"></param>
    private void DisplayConditions(int conditionIndex)
    {
        GUILayout.BeginVertical();
        GUI.color = Color.white;

        //Display loop
        for (int expressionIndex = 0; expressionIndex < target.ConditionList[conditionIndex].ExpressionList.Count; expressionIndex++)
        {
            GUILayout.BeginHorizontal();
            {
                int conceptIndex;
                conceptIndex = _conditionDisplayer.conceptIndexes[conditionIndex][expressionIndex] = EditorGUILayout.Popup(
                                _conditionDisplayer.conceptIndexes[conditionIndex][expressionIndex], _conditionDisplayer.conceptNames.ToArray());

                GUILayout.Label("IS");

                _conditionDisplayer.functionIndexes[conditionIndex][expressionIndex] = EditorGUILayout.Popup(
                                        _conditionDisplayer.functionIndexes[conditionIndex][expressionIndex],
                                        _conditionDisplayer.conceptDictionary[conceptIndex].GetFuctionNames().ToArray());

                //Draws the operator as a LABEL if it is the first one (making the OPERATOR= UNCHANGEABLE), and draw any
                //remaining operator as a ENUMPOPUP (making them CHANGEABLE)
                Operator __currentOperator = _conditionDisplayer.GetOperatorFromConditionExpression(conditionIndex, expressionIndex);
                if (expressionIndex == 0)
                {
                    GUILayout.Label(_conditionDisplayer.operatorList[conditionIndex][expressionIndex].ToString());
                }
                else
                {
                    __currentOperator = _conditionDisplayer.SetOperatorForCunrrentConditionExpression(conditionIndex, expressionIndex, (Operator)EditorGUILayout.EnumPopup(_conditionDisplayer.operatorList[conditionIndex][expressionIndex]));
                }

                //Draws expressions in red color if they appear after a END operator
                if (__currentOperator == Operator.END)
                {
                    GUI.color = Color.HSVToRGB(1.0f, 0.6f, 1.0f);
                }
            }
            GUILayout.EndHorizontal();
        }

        GUI.color = Color.white;

        GUILayout.EndVertical();

        GUILayout.Space(EditorCommons._defaultSpacing/2);
    }

    /// <summary>
    /// Updates the conditions in the refeced FuzzyBehaviour. Pass the values selected by the user, from the inspector to the class instance
    /// </summary>
    private void UpdateConditions()
    {
        if (target.ConceptList == null)
            return;

        for (int i = 0; i < target.ConditionList.Count; i++)
        {
            for (int j = 0; j < target.ConditionList[i].ExpressionList.Count; j++)
            {
                target.ConditionList[i].ExpressionList[j].Concept = _conditionDisplayer.GetConceptFomConditionExnpression(i, j);

                target.ConditionList[i].ExpressionList[j].Function = _conditionDisplayer.GetFunctionFromConditionExpression(i, j);

                target.ConditionList[i].ExpressionList[j].Operator = _conditionDisplayer.GetOperatorFromConditionExpression(i, j);
            }
        }
    }

    private void UpdateObjectPicker()
    {
        EditorUtility.SetDirty(base.target);
        EditorUtility.SetDirty(this);

        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            Debug.Log("ID: " + EditorGUIUtility.GetObjectPickerControlID());

            switch (EditorGUIUtility.GetObjectPickerControlID())
            {

                case (int)EditorCommons.WindowCommands.AddConcept:
                    {
                        Concept concept = (Concept)EditorGUIUtility.GetObjectPickerObject();

                        if (!target.ContainsConcept(concept))
                        {
                            target.AddConcept(concept);

                            _displayFunctions.Add(false);

                            ConditionDisplayer.ReloadConcepts();
                            ConditionDisplayer.ReloadConditions();

                            _showConcepts = true;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Fuzzy warning", "Concept already added to related concepts list", "Ok");
                        }
                    }
                    break;

                case (int)EditorCommons.WindowCommands.RemoveConcept:
                    {

                    }
                    break;
            }
        }
    }

    #endregion


    #region Functionality methods

    /// <summary>
    /// Buttons and method calls of the concept removal logic
    /// </summary>
    private void RemoveConceptFunctionality()
    {
        if (_conditionDisplayer == null)
        {
            _conditionDisplayer = new EditorConditionDisplay(target);
        }

        GUILayout.BeginHorizontal();
        {
            if(target.ConceptList.Count > 0)
            {
                if (GUILayout.Button("Remove Concept", GUILayout.Width(150)))
                    RemoveConceptFromTargetList();

                _selectedConcept = EditorGUILayout.Popup(_selectedConcept, _conditionDisplayer.conceptNames.ToArray());
            }
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Clears all FuzzyBehaviour's containers 
    /// </summary>
    private void ClearButton()
    {
        if (GUILayout.Button("Clear"))
        {
            target.ConditionList.Clear();
            target.ConceptList.Clear();
            target.ConceptList = null;

            _displayFunctions = new List<bool>();
        }
    }

    /// <summary>
    /// Metod that implements the logic of addition and removal of expressions from the condtiitons
    /// </summary>
    /// <param name="conditionIndex"></param>
    private void AddRemoveExpressionFunctionality(int conditionIndex)
    {
        if(GUILayout.Button("+", GUILayout.Width(50)))
        {
            int __funcIndex = 0;
            int __concIndex = 0;
            Operator __selectedOperator =   target.ConditionList[conditionIndex].ExpressionList.Count <= 0 ?
                                            Operator.EQUALS : Operator.END;
            
            //Adds the condition expression to the current condition
            target.ConditionList[conditionIndex].AddExpression(new ConditionExpression(
                                        _conditionDisplayer.GetConceptByIndex(__concIndex),
                                        _conditionDisplayer.GetFunctionFromConceptByIndex(_conditionDisplayer.CurrentConcept, __funcIndex),
                                        __selectedOperator));

            //Adds the current concept and function INDEX to the SLECTED concept and function list
            _conditionDisplayer.AddConceptInCurrentConditionExpressionList(conditionIndex, __concIndex);
            _conditionDisplayer.AddFunctionInCurrentConditionExpressionList(conditionIndex, __funcIndex);
            _conditionDisplayer.AddOperatorInCurrentConditionExpressionList(conditionIndex, __selectedOperator);
            
        }
        else if (GUILayout.Button("-", GUILayout.Width(50)))
        {
            if(target.ConditionList[conditionIndex].ExpressionList.Count > 0)
            {
                _conditionDisplayer.RemoveConditionExpression(conditionIndex);
                target.ConditionList[conditionIndex].RemoveExpression();
            }
        }
    }

    /// <summary>
    /// Buttons related to condition creation functionality
    /// </summary>
    private void CreateConditionFunctionality()
    {
        if (target.ConceptList.Count <= 0)
            return;

        if (GUILayout.Button("Create contition"))
        {
            if (!_conditionDisplayer)
                _conditionDisplayer = new EditorConditionDisplay(target);


            target.CreateCondition(_conditionDisplayer.conceptDictionary[0],
                                    _conditionDisplayer.conceptDictionary[0].Functions[0],
                                    Operator.EQUALS);

            _conditionDisplayer.ReloadConditions();

            _displayConditions = true;
        }

        if(GUILayout.Button("Remove condition"))
        {
            _conditionDisplayer.RemoveCondition();
            target.RemoveCondition();
        }
    }

    private void DisplayConditionFunctionality()
    {
        GUILayout.Space(EditorCommons._defaultSpacing);

        //Display part. Foldout list of conditions
        if (target.ConditionList != null)
        {
            if (target.ConditionList.Count > 0)
            {
                if (_displayConditions = EditorGUILayout.Foldout(_displayConditions, "Condition list"))
                {
                    if (_conditionDisplayer == null)
                    {
                        _conditionDisplayer = new EditorConditionDisplay(target);
                    }

                    for (int i = 0; i < target.ConditionList.Count; i++)
                    {
                        GUI.color = ConditionColor[i];
                        GUILayout.BeginVertical("Box");
                        {
                            GUILayout.Label("Condition " + i);

                            //UI function that displays the conditions 
                            DisplayConditions(i);

                            //Buttons in the UI to add and remove expressions from the conditions
                            AddRemoveExpressionFunctionality(i);
                        }
                        GUILayout.EndVertical();
                    }

                    GUI.color = Color.white;
                    UpdateConditions();
                }
            }
        }
    }

    #endregion

    private FuzzyBehaviourConfig CreateConfigInstance()
    {
        FuzzyBehaviourConfig __config = CreateInstance<FuzzyBehaviourConfig>();

        string __assetPath = "Assets/FuzzyConfig";
        if (!System.IO.Directory.Exists(__assetPath))
        {
            System.IO.Directory.CreateDirectory(__assetPath);
        }

        __assetPath = AssetDatabase.GenerateUniqueAssetPath(__assetPath + "/" + target.gameObject.name + " config.asset");

        AssetDatabase.CreateAsset(__config, __assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return (__config);
    }
    private FuzzyBehaviourConfig FindConfigFile()
    {
        string __assetPath = "Assets/FuzzyConfig";
        if (System.IO.Directory.Exists(__assetPath))
        {
            if (System.IO.File.Exists(__assetPath + "/" + target.gameObject.name + " config.asset"))
            {
                return ((FuzzyBehaviourConfig)AssetDatabase.LoadAssetAtPath(__assetPath + "/" + target.gameObject.name + " config.asset", typeof(FuzzyBehaviourConfig)));
            }
        }

        return null;
    }
    private void DisplayObjectsReferences()
    {
        GUILayout.Space(EditorCommons._defaultSpacing);
        if (target.Config == null)
        {
            FuzzyBehaviourConfig __configFile = FindConfigFile();
            if (__configFile != null)
            {
                __configFile = CreateConfigInstance();
                target.Config = __configFile;
            }
        }
        else
        {
            EditorGUILayout.ObjectField(target.Config, typeof(FuzzyBehaviourConfig), false);
        }
    }

    #region Unity Messages

    public void OnEnable()
    {
        target = (FuzzyBehaviour)base.target;

        _conditionDisplayer = new EditorConditionDisplay(target);

        if (_displayFunctions == null)
            _displayFunctions = new List<bool>();

        if (target.ConceptList.Count != _displayFunctions.Count)
        {
            _displayFunctions.Clear();
            foreach (Concept concept in target.ConceptList)
            {
                _displayFunctions.Add(false);
            }
        }
    }
    public override void OnInspectorGUI()
    {
        ClearButton();

        UpdateObjectPicker();

        DisplayObjectsReferences();

        GenerateRandomConditionColor();

        DisplayConceptAndFunctions();

        AddConceptFunctionality();

        RemoveConceptFunctionality();

        DisplayConditionFunctionality();

        CreateConditionFunctionality();
    }

    #endregion
}
