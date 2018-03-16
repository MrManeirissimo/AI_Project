using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class FuzzyGraphEditorWindow : EditorWindow
{
    static List<AnimationCurve> curveList;

    SerializedProperty property;

    [MenuItem("Fuzzy/Curve editor")]
    public static void CreateWindow()
    {
        GetWindow<FuzzyGraphEditorWindow>("Fuzzy Curve Editor");
    }

    FuzzyGraphEditorWindow()
    {
        curveList = new List<AnimationCurve>();
        curveList.Add(new AnimationCurve(new Keyframe[3] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0) }));
        curveList.Add(new AnimationCurve(new Keyframe[3] { new Keyframe(0, 0), new Keyframe(0.5f, 0.5f), new Keyframe(1, 1) }));
    }
    private void OnEnable()
    {
        
    }
    private void OnGUI()
    {
        foreach (var item in curveList)
        {
            EditorGUILayout.CurveField(item, Random.ColorHSV(), new Rect(0,0,800,600));
        }
    }
}