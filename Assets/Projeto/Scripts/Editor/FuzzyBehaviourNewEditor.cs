using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Fuzzy;

[CustomEditor(typeof(Fuzzy.FuzzyBehaviour))]
public class FuzzyBehaviourNewEditor : Editor
{
    private new FuzzyBehaviour target;
    private static EditorConditionDisplay displayer;

    private static int selectedConcept = -1;

    #region Methods

    //Utility
    private void ClearButton()
    {
        if (GUILayout.Button("Clear"))
        {
            target.ConditionList.Clear();
            target.ConceptList.Clear();
            target.ConceptList = null;

            if(target.Config)
            {
                target.Config.ColorList.Clear();
                target.ConditionList.Clear();
            }
        }
    }
    private void UpdateObjectPicker()
    {
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            switch (EditorGUIUtility.GetObjectPickerControlID())
            {
                case (int)EditorCommons.WindowCommands.AddConcept:
                    {
                        Concept concept = (Concept)EditorGUIUtility.GetObjectPickerObject();
                        if (concept == null)
                            return;

                        if (!target.ContainsConcept(concept))
                        {
                            target.AddConcept(concept);

                            displayer = new EditorConditionDisplay(target)
                            {
                                DisplayConcepts = true
                            };

                            EditorUtility.SetDirty(target);
                            EditorUtility.SetDirty(target.gameObject);
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                        }
                    }
                    break;
            }
        }
    }

    //Config file detection display and creation
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
        if(System.IO.Directory.Exists(__assetPath))
        {
            if(System.IO.File.Exists(__assetPath + "/" + target.gameObject.name + " config.asset"))
            {
                Object __configFile = AssetDatabase.LoadAssetAtPath(__assetPath + "/" + target.gameObject.name + " config.asset", typeof(FuzzyBehaviourConfig));
                return ((FuzzyBehaviourConfig)__configFile);
            }
        }

        return null;
    }
    private void UpdateAndDisplayConfigReference()
    {
        GUILayout.Space(EditorCommons._defaultSpacing / 2);

        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true);

        if(target.Config == null)
        {
            FuzzyBehaviourConfig __configFile = FindConfigFile();
            if(__configFile == null)
            {
                __configFile = CreateConfigInstance();
                target.Config = __configFile;
            }
        }
        else
        {
            target.Config.Behaviour = target as FuzzyBehaviour;
            EditorGUILayout.ObjectField("Config file", target.Config, typeof(FuzzyBehaviourConfig), false);
        }
        GUILayout.Space(EditorCommons._defaultSpacing / 2);
    }

    //Concept adition, removal and display
    private void AddConceptFunctionality()
    {
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Add concept"))
            {
                EditorGUIUtility.ShowObjectPicker<Concept>(new UnityEngine.Object(), false, string.Empty, (int)EditorCommons.WindowCommands.AddConcept);
            }
        }
        GUILayout.EndHorizontal();
    }
    private void RemoveConceptFunctionality()
    {
        if (displayer == null)
            displayer = new EditorConditionDisplay(target);

        GUILayout.BeginHorizontal();
        {
            if (target.ConceptList.Count > 0)
            {
                if (GUILayout.Button("Remove Concept", GUILayout.Width(150)))
                    RemoveConceptFromTargetList();

                selectedConcept = EditorGUILayout.Popup(selectedConcept, displayer.conceptNames.ToArray());
            }
        }
        GUILayout.EndHorizontal();
    }
    private void RemoveConceptFromTargetList()
    {
        if (selectedConcept >= target.ConceptList.Count || selectedConcept < 0)
            throw new System.Exception("@Fuzzy Editor Exception - Index out of range from conceptList");

        target.RemoveConcept(selectedConcept);

        displayer.ReloadConcepts();

        selectedConcept = -1;
    }
    private void DisplayConceptAndFunctions()
    {
        if (displayer.DisplayConcepts = EditorGUILayout.Foldout(displayer.DisplayConcepts, "Concept List"))
        {
            for (int index = 0; index < target.ConceptList.Count; index++)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        displayer.SetDisplayFunctionOfConceptState(
                            EditorGUILayout.Foldout(displayer.DisplayFunctionsOfConcept(index), displayer.GetConceptName(index)),
                            index);

                        EditorGUILayout.ObjectField(target.ConceptList[index], typeof(Concept), false);

                        //Editable box with the concept value
                        if(target.SetConceptValue(index, EditorGUILayout.FloatField(target.ConceptList[index].Value, GUILayout.Width(50))))
                        {
                            EditorUtility.SetDirty(target.GetConcept(index));
                        }
                    }
                    GUILayout.EndHorizontal();


                    if (displayer.DisplayFunctionsOfConcept(index))
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

    //Conditions
    private void DisplayConditionFunctionality()
    {
        GUILayout.Space(EditorCommons._defaultSpacing);

        //Display part. Foldout list of conditions
        if (target.ConditionList != null)
        {
            if (target.ConditionList.Count > 0)
            {
                if (displayer.DisplayConditions = EditorGUILayout.Foldout(displayer.DisplayConditions, "Condition list"))
                {
                    for (int i = 0; i < target.ConditionList.Count; i++)
                    {
                        if (i < target.Config.ColorList.Count)
                            GUI.color = target.Config.ColorList[i];

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

                    UpdateConditions();
                    GUI.color = Color.white;
                }
            }
        }
    }
    private void DisplayConditions(int condIndx)
    {
        GUILayout.BeginVertical();
        GUI.color = Color.white;

        //Display loop
        for (int expIndx = 0; expIndx < target.ConditionList[condIndx].ExpressionList.Count; expIndx++)
        {
            GUILayout.BeginHorizontal();
            {
                int conceptIndex;
                conceptIndex =  displayer.conceptIndexes[condIndx][expIndx] = EditorGUILayout.Popup(
                                displayer.conceptIndexes[condIndx][expIndx], displayer.conceptNames.ToArray());

                GUILayout.Label("IS");

                displayer.functionIndexes[condIndx][expIndx] = EditorGUILayout.Popup(
                                        displayer.functionIndexes[condIndx][expIndx],
                                        displayer.conceptDictionary[conceptIndex].GetFuctionNames().ToArray());

                //Draws the operator as a LABEL if it is the first one (making the OPERATOR= UNCHANGEABLE), and draw any
                //remaining operator as a ENUMPOPUP (making them CHANGEABLE)
                Operator __currentOperator = displayer.GetOperatorFromConditionExpression(condIndx, expIndx);
                if (expIndx == 0)
                {
                    GUILayout.Label(displayer.operatorList[condIndx][expIndx].ToString());
                }
                else
                {
                    __currentOperator = displayer.SetOperatorForCunrrentConditionExpression(
                                        condIndx,
                                        expIndx,
                                        (Operator)EditorGUILayout.EnumPopup(displayer.operatorList[condIndx][expIndx]));
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
        GUILayout.Space(EditorCommons._defaultSpacing / 2);
    }
    private void CreateConditionFunctionality()
    {
        if (target.ConceptList.Count <= 0)
            return;

        if (GUILayout.Button("Create contition"))
        {
            target.CreateCondition(displayer.conceptDictionary[0],
                                    displayer.conceptDictionary[0].Functions[0],
                                    Operator.EQUALS);

            if(target.Config)
            {
                target.Config.GetNewColor();
                EditorUtility.SetDirty(target.Config);
            }            

            displayer.ReloadConditions();
            displayer.DisplayConditions = true;

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Remove condition"))
        {
            displayer.RemoveCondition();
            target.RemoveCondition();

            displayer.ReloadConditions();
            EditorUtility.SetDirty(target);

            target.Config.ColorList.RemoveAt(target.Config.ColorList.Count - 1);
            EditorUtility.SetDirty(target.Config);
        }
    }
    private void AddRemoveExpressionFunctionality(int cndIndx)
    {
        if (GUILayout.Button("+", GUILayout.Width(50)))
        {
            int __funcIndex = 0;
            int __concIndex = 0;
            Operator __selectedOperator = Operator.END;

            //Adds the condition expression to the current condition

            ConditionExpression exp = new ConditionExpression(
                displayer.GetConceptByIndex(__concIndex),
                displayer.GetFunctionFromConceptByIndex(displayer.CurrentConcept, __funcIndex),
                __selectedOperator);

            target.ConditionList[cndIndx].AddExpression(exp);

            //if(target.Config != null) target.Config.ConditionList[cndIndx].AddExpression(exp);

            //Adds the current concept and function INDEX to the SELECTED concept and function list
            displayer.AddConceptInCurrentConditionExpressionList(cndIndx, __concIndex);
            displayer.AddFunctionInCurrentConditionExpressionList(cndIndx, __funcIndex);
            displayer.AddOperatorInCurrentConditionExpressionList(cndIndx, __selectedOperator);

            EditorUtility.SetDirty(target);
        }

        else if (GUILayout.Button("-", GUILayout.Width(50)))
        {
            if (target.ConditionList[cndIndx].ExpressionList.Count > 0)
            {
                displayer.RemoveConditionExpression(cndIndx);
                target.ConditionList[cndIndx].RemoveExpression();

                EditorUtility.SetDirty(target);
            }
            else
            {
                Debug.LogWarning("@FuzzyBehaviourEditor - Cannot delete inicial condition expression. Delete condition instead");
            }
        }
    }
    private void UpdateConditions()
    {
        if (target.ConceptList == null)
            return;

        for (int i = 0; i < target.ConditionList.Count; i++)
        {
            for (int j = 0; j < target.ConditionList[i].ExpressionList.Count; j++)
            {
                target.ConditionList[i].ExpressionList[j].Concept = displayer.GetConceptFomConditionExnpression(i, j);

                target.ConditionList[i].ExpressionList[j].Function = displayer.GetFunctionFromConditionExpression(i, j);

                target.ConditionList[i].ExpressionList[j].Operator = displayer.GetOperatorFromConditionExpression(i, j);
            }
        }
    }

    #endregion

    #region Unity messages

    public void OnEnable()
    {
        target = (FuzzyBehaviour)base.target;
        if (displayer == null && target != null) displayer = new EditorConditionDisplay(target);

        if (target.Config == null)
        {
            FuzzyBehaviourConfig __configFile = FindConfigFile();
            if (__configFile != null)
            {
                __configFile = CreateConfigInstance();
                target.Config = __configFile;
            }
        }
    }
    public override void OnInspectorGUI()
    {
        UpdateObjectPicker();

        ClearButton();

        UpdateAndDisplayConfigReference();

        DisplayConceptAndFunctions();

        AddConceptFunctionality();

        RemoveConceptFunctionality();

        DisplayConditionFunctionality();

        CreateConditionFunctionality();
    }

    #endregion
}

