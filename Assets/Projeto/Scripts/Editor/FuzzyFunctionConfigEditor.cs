using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Fuzzy.FunctionConfig))]
public class FuzzyFunctionConfigEditor : Editor
{
    private Fuzzy.FunctionConfig _self;
    private static float _defaultSpacing = 20;

    private delegate void FuzzyFuncDel(Fuzzy.Function function);
    FuzzyFuncDel FuzzyFuncBody = (Fuzzy.Function fuzzyFunction) =>
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(_defaultSpacing);

            fuzzyFunction.Func = EditorGUILayout.CurveField(fuzzyFunction.Func,
                                    GUILayout.Height(300),
                                    GUILayout.Width(350));

            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };
    FuzzyFuncDel FuzzyBeginCoord = (Fuzzy.Function func) =>
    {
        GUILayout.BeginHorizontal();
        {
            float __xValue, __yValue;
            GUILayout.Space(_defaultSpacing);
            GUILayout.Label("Begin: ", GUILayout.Width(100));

            GUILayout.Label("X: ", GUILayout.Width(_defaultSpacing));
            if (float.TryParse((GUILayout.TextField(func.Begin.X.ToString())), out __xValue))
            {
                func.Begin.X = __xValue;
            }

            GUILayout.Label("Y: ", GUILayout.Width(_defaultSpacing));
            if (float.TryParse(GUILayout.TextField(func.Begin.Y.ToString()), out __yValue))
            {
                func.Begin.Y = __yValue;
            }
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };
    FuzzyFuncDel FuzzyEndCoord = (Fuzzy.Function func) =>
    {
        GUILayout.BeginHorizontal();
        {
            float __xValue, __yValue;
            GUILayout.Space(_defaultSpacing);
            {
                GUILayout.Label("End: ", GUILayout.Width(100));
                GUILayout.Label("X: ", GUILayout.Width(_defaultSpacing));
                if (float.TryParse(GUILayout.TextField(func.End.X.ToString()), out __xValue))
                {
                    func.End.X = __xValue;
                }

                GUILayout.Label("Y: ", GUILayout.Width(_defaultSpacing));
                if (float.TryParse(GUILayout.TextField(func.End.Y.ToString()), out __yValue))
                {
                    func.End.Y = __yValue;
                }
            }
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };

    private void DisplayInspectorHeader()
    {
        //Name display box
        GUILayout.BeginVertical("box");
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(150);
                if(_self && _self.Func)
                    GUILayout.Label(_self.Func.Name);
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
            if (_self && _self.Func)
                _self.Func.Name = GUILayout.TextField(_self.Func.Name, GUILayout.Width(250));
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        _self = (Fuzzy.FunctionConfig)target;
    }
    public override void OnInspectorGUI()
    {
        if (_self && _self.Func)
        {
            DisplayInspectorHeader();

            FuzzyFuncBody(_self.Func);

            FuzzyBeginCoord(_self.Func);

            FuzzyEndCoord(_self.Func);

            _self.Func.UpdateAnimationCurveValues();
        }
    }
}
