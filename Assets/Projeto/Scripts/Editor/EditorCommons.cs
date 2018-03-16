using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class EditorConditionDisplay
{
    public class TwoWayList<T, U>
    {
        public class KeyValuePair
        {
            private T _key;
            private U _value;

            public T Key { get { return _key; } }
            public U Value { get { return _value; } }

            public KeyValuePair(T key, U value)
            {
                this._key = key;
                this._value = value;
            }

            public static implicit operator bool(KeyValuePair val)
            {
                return (val != null ? true : false);
            }
        }

        private List<T> _keys;
        private List<U> _values;

        public TwoWayList()
        {
            _keys = new List<T>();
            _values = new List<U>();
        }
        public TwoWayList(List<T> keys, List<U> values)
        {
            this._keys = keys;
            this._values = values;
        }

        public KeyValuePair Add(T key, U value)
        {
            _keys.Add(key);
            _values.Add(value);

            return (new KeyValuePair(key, value));
        }
        public KeyValuePair Remove(T key)
        {
            int index = _keys.IndexOf(key);
            if(index >= 0)
            {
                KeyValuePair pair = new KeyValuePair(_keys[index], _values[index]);
                _values.Remove(this[key]);
                _keys.Remove(key);

                return (pair);
            }

            return (null);
        }
        public KeyValuePair Remove(U value)
        {
            int index = _values.IndexOf(value);
            if(index >= 0 )
            {
                KeyValuePair pair = new KeyValuePair(_keys[index], _values[index]);
                _keys.Remove(this[value]);
                _values.Remove(value);

                return (pair);
            }

            return (null);
        }

        public bool Contains(T key)
        {
            return (_keys.Contains(key));
        }
        public bool Contains(U value)
        {
            return (_values.Contains(value));
        }

        public T this[U value]
        {
            get
            {
                if(_keys != null && _values != null)
                {
                    int __keysIndex = _values.IndexOf(value);
                    if (__keysIndex >= 0)
                        return (_keys[__keysIndex]);
                }

                return default(T);
            }
        }
        public U this[T key]
        {
            get
            {
                if (_keys != null && _values != null)
                {
                    int __valueIndex = _keys.IndexOf(key);
                    if (__valueIndex >= 0)
                        return (_values[__valueIndex]);
                }

                return default(U);
            }
        }
    }

    public TwoWayList<int, Fuzzy.Concept> conceptDictionary;
    public Dictionary<Fuzzy.Concept, TwoWayList<int, Fuzzy.Function>> functionDictionary;

    public List<List<int>> conceptIndexes;
    public List<List<int>> functionIndexes;
    public List<bool> displayFunctions;
    
    public List<string> conditionNames;
    public List<string> conceptNames;

    public List<List<Fuzzy.Operator>> operatorList;

    private Fuzzy.FuzzyBehaviour _owner;
    private Fuzzy.Concept _currentConcept;
    private Fuzzy.Function _currentFunction;
    private Fuzzy.Operator _currentOperator;

    #region Public access functions

    public bool DisplayConcepts
    {
        get; set;
    }
    public bool DisplayConditions
    {
        get; set;
    }

    public bool DisplayFunctionsOfConcept(int index)
    {
        if (index >= displayFunctions.Count || index < 0)
            throw new System.Exception("@Condition displayer - index out of range in displayFunctions");

        return (displayFunctions[index]);
    }
    public void SetDisplayFunctionOfConceptState(bool value, int index)
    {
        if (index >= displayFunctions.Count || index < 0)
            throw new System.Exception("@Condition displayer - index out of range in displayFunctions");

        displayFunctions[index] = value;
    }

    public string GetConceptName(int index)
    {
        if (index >= conceptNames.Count || index < 0)
            throw new System.Exception("@Condition displayer - index out of range in conceptNames");

        return (conceptNames[index]);
    }

    public int GetConceptIndex(Fuzzy.Concept concept)
    {
        if (conceptDictionary.Contains(concept))
            return (conceptDictionary[concept]);

        return (-1);
    }
    public int GetFunctionIndexFromConcept(Fuzzy.Function function, Fuzzy.Concept inConcept)
    {
        if (inConcept.GetFunction(function))
        {
            for (int i = 0; i < inConcept.Functions.Count; i++)
            {
                if (function.Equals(inConcept.Functions[i]))
                    return (i);
            }
        }

        return (default(int));
    }

    public Fuzzy.Concept GetConceptFomConditionExnpression(int conditionIndex, int expressionIndex)
    {
        _currentConcept = null;
        _currentConcept = GetConceptByIndex(conceptIndexes[conditionIndex][expressionIndex]);

        return (_currentConcept);
    }
    public Fuzzy.Concept GetConceptByIndex(int index)
    {
        _currentConcept = null;
        try
        {
            _currentConcept = conceptDictionary[index];
        }
        catch(KeyNotFoundException)
        {
            Debug.LogWarning("@" + GetType() + " - Key exception. Could not find the key inside concept dictionary");
        }

        return (_currentConcept);
    }
    public Fuzzy.Function GetFunctionFromConceptByIndex(Fuzzy.Concept concept, int index)
    {
        _currentFunction = null;
        try
        {
            _currentFunction = functionDictionary[concept][index];
        }
        catch(KeyNotFoundException)
        {
            Debug.LogWarning("@" + GetType() + " - Key exception. Could not find the key inside function dictionary");
        }
        catch(System.IndexOutOfRangeException)
        {
            Debug.LogWarning("@" + GetType() + " - Index exception. Invalid index for selected concept's function list");
        }

        return (_currentFunction);
    }
    public Fuzzy.Function GetFunctionFromConditionExpression(int conditionIndex, int expressionIndex)
    {
        _currentFunction = null;
        _currentFunction = GetFunctionFromConceptByIndex(
                                    GetConceptByIndex(conceptIndexes[conditionIndex][expressionIndex]),
                                    functionIndexes[conditionIndex][expressionIndex]);

        return (_currentFunction);
    }

    public Fuzzy.Operator GetOperatorFromConditionExpression(int conditionIndex, int expressionIndex)
    {
        _currentOperator = Fuzzy.Operator.END;
        try
        {
            _currentOperator = operatorList[conditionIndex][expressionIndex];
        }
        catch(System.ArgumentOutOfRangeException)
        {
            Debug.LogError("@" + GetType() + " - Index exception. Invalid index for condition or expression list");
        }

        return (_currentOperator);
    }

    public Fuzzy.Concept SetConceptForCurrentConditionExpression(int conditionIndex, int expressionIndex, int conceptIndex)
    {
        try
        {
            conceptIndexes[conditionIndex][expressionIndex] = conceptIndex;

            _currentConcept = null;
            _currentConcept = GetConceptByIndex(conceptIndex);
        }
        catch(System.ArgumentOutOfRangeException)
        {
            Debug.LogError("@" + GetType() + " - Index exception. Invalid index for condition or expression list");
        }

        return (_currentConcept);
    }
    public Fuzzy.Function SetFunctionForCurrentConditionExpression(int conditionIndex, int expresssionIndex, int functionIndex)
    {
        try
        {
            functionIndexes[conditionIndex][expresssionIndex] = functionIndex;
            _currentFunction = null;
            _currentFunction = GetConceptByIndex(conceptIndexes[conditionIndex][expresssionIndex]).Functions[functionIndex];
        }
        catch (System.ArgumentOutOfRangeException)
        {
            Debug.LogError("@" + GetType() + " - Index exception. Invalid index for condition or expression list");
        }

        return (_currentFunction);
    }
    public Fuzzy.Operator SetOperatorForCunrrentConditionExpression(int conditionIndex, int expressionIndex, Fuzzy.Operator op)
    {
        try
        {
            operatorList[conditionIndex][expressionIndex] = op;

            _currentOperator = 0;
            _currentOperator = op;
        }
        catch (System.ArgumentOutOfRangeException)
        {
            Debug.LogError("@" + GetType() + " - Index exception. Invalid index for condition or expression list");
        }

        return (_currentOperator);
    }

    public Fuzzy.Concept AddConceptInCurrentConditionExpressionList(int conditionIndex, int conceptIndex)
    {
        conceptIndexes[conditionIndex].Add(conceptIndex);

        _currentConcept = null;
        _currentConcept = GetConceptByIndex(conceptIndex);

        return (_currentConcept);
    }
    public Fuzzy.Function AddFunctionInCurrentConditionExpressionList(int conditionIndex, int functionIndex)
    {
        functionIndexes[conditionIndex].Add(functionIndex);

        _currentFunction = null;
        _currentFunction = GetConceptByIndex(conceptIndexes[conditionIndex][functionIndexes[conditionIndex].Count -1 ]).Functions[functionIndex];

        return (_currentFunction);
    }
    public Fuzzy.Operator AddOperatorInCurrentConditionExpressionList(int conditionIndex, Fuzzy.Operator op)
    {
        operatorList[conditionIndex].Add(op);

        return(_currentOperator = op);
    }

    public void RemoveConditionExpression(int conditionIndex)
    {
        if(conceptIndexes[conditionIndex].Count > 0 && functionIndexes[conditionIndex].Count > 0 && operatorList[conditionIndex].Count > 0 )
        {
            conceptIndexes[conditionIndex].RemoveAt(conceptIndexes[conditionIndex].Count - 1);
            functionIndexes[conditionIndex].RemoveAt(functionIndexes[conditionIndex].Count - 1);
            operatorList[conditionIndex].RemoveAt(operatorList[conditionIndex].Count - 1);
        }
    }
    public void RemoveCondition()
    {
        conceptIndexes.RemoveAt(conceptIndexes.Count - 1);
        functionIndexes.RemoveAt(functionIndexes.Count - 1);
        operatorList.RemoveAt(operatorList.Count - 1);
    }

    public void RemoveConceptName(Fuzzy.Concept concept)
    {
        foreach (var name in conceptNames)
        {
            if (name == concept.name)
            {
                conceptNames.Remove(name);
                break;
            }
        }
    }

    public void ResetExpressionsUsingConcept(int conceptIndex)
    {
        for (int i = 0; i < conceptIndexes.Count; i++)
        {
            for (int j = 0; j < conceptIndexes[i].Count; j++)
            {
                if (conceptIndexes[i][j] == conceptIndex)
                {
                    conceptIndexes[i][j] = 0;
                    functionIndexes[i][j] = 0;
                }
            }
        }
    }
    public void ResetExpressionsUsingConcept(Fuzzy.Concept concept)
    {
        for (int i = 0; i < conceptIndexes.Count; i++)
        {
            for (int j = 0; j < conceptIndexes[i].Count; j++)
            {
                if(conceptIndexes[i][j] == conceptDictionary[concept])
                {
                    conceptIndexes[i][j] = 0;
                    functionIndexes[i][j] = 0;
                }
            }
        }
    }

    public void AddConceptName(string name)
    {
        if(conceptNames == null)
            conceptNames = new List<string>();

        conceptNames.Add(name);
    }

    #endregion

    #region Accessors

    public Fuzzy.Concept CurrentConcept
    {
        get
        {
            return (_currentConcept);
        }
    }
    public Fuzzy.Function CurrentFunction
    {
        get
        {
            return (_currentFunction);
        }
    }
    public Fuzzy.Operator CurrentOperator
    {
        get
        {
            return (_currentOperator);
        }
    }

    #endregion

    #region Ctor

    public EditorConditionDisplay(Fuzzy.FuzzyBehaviour behaviour)
    {
        _owner = behaviour;

        DisplayConcepts = true;

        ReloadConcepts();

        ReloadConditions();
    }

    #endregion

    #region Core methods

    public void ReloadConcepts()
    {
        conceptNames = new List<string>();
        conceptDictionary = new TwoWayList<int, Fuzzy.Concept>();

        displayFunctions = new List<bool>();
        functionDictionary = new Dictionary<Fuzzy.Concept, TwoWayList<int, Fuzzy.Function>>();        

        //Fills the conceptDictionary container and gather the concept names
        for (int conceptKey = 0; conceptKey < _owner.ConceptList.Count; conceptKey++)
        {
            Fuzzy.Concept concept = _owner.ConceptList[conceptKey];

            displayFunctions.Add(false);
            conceptNames.Add(concept.name);
            conceptDictionary.Add(conceptKey, concept);
            functionDictionary.Add(concept, new TwoWayList<int, Fuzzy.Function>());

            //Fills the function two-way list of the current concept
            for (int j = 0; j < concept.Functions.Count; j++)
            {
                functionDictionary[concept].Add(j, concept.GetFunction(j));
            }
        }
    }
    public void ReloadConditions()
    {
        conceptIndexes = new List<List<int>>();
        functionIndexes = new List<List<int>>();
        operatorList = new List<List<Fuzzy.Operator>>();

        //If there is a condition list
        if (_owner.ConditionList == null)
            return;

        //Adds a list of indexes for concepts and functions for each condition inside condition list
        for (int i = 0; i < _owner.ConditionList.Count; i++)
        {
            conceptIndexes.Add(new List<int>());
            functionIndexes.Add(new List<int>());
            operatorList.Add(new List<Fuzzy.Operator>());

            for (int j = 0; j < _owner.ConditionList[i].ExpressionList.Count; j++)
            {
                conceptIndexes[i].Add(conceptDictionary[_owner.ConditionList[i].ExpressionList[j].Concept]);
                functionIndexes[i].Add(GetFunctionIndexFromConcept(_owner.ConditionList[i].ExpressionList[j].Function, _owner.ConditionList[i].ExpressionList[j].Concept));
                operatorList[i].Add(_owner.ConditionList[i].ExpressionList[j].Operator);
            }
        }
    }

    #endregion

    #region Operators

    public static implicit operator bool(EditorConditionDisplay displayer)
    {
        return (displayer == null ? false : true);
    }

    #endregion
}

public static class EditorCommons 
{
    internal static float _defaultSpacing = 20;
    
    internal delegate void FuzzyFuncDel(Fuzzy.Function function);
    internal static FuzzyFuncDel FuzzyFuncBody = (Fuzzy.Function fuzzyFunction) =>
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
    internal static FuzzyFuncDel FuzzyBeginCoord = (Fuzzy.Function func) =>
    {
        GUILayout.BeginHorizontal();
        {
            float __xValue, __yValue;
            GUILayout.Space(_defaultSpacing);
            GUILayout.Label("Begin: ", GUILayout.Width(100));

            GUILayout.Label("X: ", GUILayout.Width(20));
            if (float.TryParse((GUILayout.TextField(func.Begin.X.ToString())), out __xValue))
            {
                func.Begin.X = __xValue;
            }

            GUILayout.Label("Y: ", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(func.Begin.Y.ToString()), out __yValue))
            {
                func.Begin.Y = __yValue;
            }
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };
    internal static FuzzyFuncDel FuzzyEndCoord = (Fuzzy.Function func) =>
    {
        GUILayout.BeginHorizontal();
        {
            float __xValue, __yValue;

            GUILayout.Space(_defaultSpacing);
            GUILayout.Label("End: ", GUILayout.Width(100));
            GUILayout.Label("X: ", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(func.End.X.ToString()), out __xValue))
            {
                func.End.X = __xValue;
            }

            GUILayout.Label("Y: ", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(func.End.Y.ToString()), out __yValue))
            {
                func.End.Y = __yValue;
            }

            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };
    internal static FuzzyFuncDel FuzzyFuncName = (Fuzzy.Function func) =>
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(_defaultSpacing);
            GUILayout.Label("Function name: ");
            func.Name = GUILayout.TextField(func.Name);
            GUILayout.Space(_defaultSpacing);
        }
        GUILayout.EndHorizontal();
    };

    internal enum WindowCommands
    {
        AddConcept = 1 << 0,
        RemoveConcept = 1 << 1
    }
}