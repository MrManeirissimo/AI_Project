using System.Collections.Generic;
using UnityEngine;

namespace Fuzzy
{
    internal static class Logic
    {
        public static float MIN(float a, float b)
        {
            return (a < b ? a : b);
        }

        public static float MAX(float a, float b)
        {
            return (a > b ? a : b);
        }


        public static float NOT(float a)
        {
            return (1 - a);
        }

        public static float AND(float a, float b)
        {
            return (MIN(a, b));
        }

        public static float OR(float a, float b)
        {
            return (MAX(a, b));
        }
    }

    public enum Operator
    {
        NOT, AND, OR, LESS, LESS_EQUAL, MORE, MORE_EQUAL, EQUALS, MIN, MAX, END
    }

    [System.Serializable]
    public class FKey
    {
        [SerializeField] private float _x;
        [SerializeField] private float _y;

        public float X
        {
            get { return (_x); }
            set { _x = value; }
        }
        public float Y
        {
            get { return (_y); }
            set { _y = value; }
        }

        public FKey()
        {
            _x = _y = 0;
        }
        public FKey(Keyframe key)
        {
            _x = key.time;
            _y = key.value;
        }
        public FKey(Vector2 coord)
        {
            _x = coord.x;
            _y = coord.y;
        }

        public static implicit operator FKey(Keyframe frame)
        {
            return (new FKey(frame));
        }
        public static implicit operator Keyframe(FKey key)
        {
            return (new Keyframe(key.X, key.Y));
        }
    }

    [System.Serializable]
    public class Function
    {
        #region Protected fields

        [Header("Function name")]
        [SerializeField] protected string _name;

        [Header("Function graph")]
        [SerializeField] protected AnimationCurve _function;

        [Header("Function values")]
        [SerializeField] protected float _fuzzy = -1;

        [Header("Keys")]
        [SerializeField] protected FKey _begin;
        [SerializeField] protected FKey _end;

        #endregion

        #region Operators

        public static implicit operator AnimationCurve(Function func)
        {
            return (func._function);
        }
        public static implicit operator bool(Function func)
        {
            if (func != null)
                return (func._function != null ? true : false);

            return false;
        }

        //public static bool operator !=(Function x, Function y)
        //{
        //    if (object.ReferenceEquals(x, null) || (object.ReferenceEquals(y, null)))
        //    {
        //        return (false);
        //    }

        //    return (!x.Equals(y));
        //}
        //public static bool operator ==(Function x, Function y)
        //{
        //    if (object.ReferenceEquals(x, null))
        //    {
        //        return (object.ReferenceEquals(y, null));
        //    }

        //    return (x.Equals(y));
        //}

        public override bool Equals(object obj)
        {
            Function func = (Function)obj;
            if(func)
            {
                return (_name == func._name && CompareCurveKeyframes(func));
            }

            return false;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Function name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The begin keyframe representation
        /// </summary>
        public FKey Begin
        {
            get { return (_begin); }
        }

        /// <summary>
        /// The end keyframe representation
        /// </summary>
        public FKey End
        {
            get { return (_end); }
        }

        /// <summary>
        /// The animation curve itself. The graph.
        /// </summary>
        public AnimationCurve Func
        {
            get { return (_function); }
            set { _function = value; }
        }

        /// <summary>
        /// The fuzzy value of the function
        /// </summary>
        public float Fuzzy
        {
            get
            {
                if (_fuzzy < 0)
                    Debug.LogWarning("@FuzzyFunction - Warning: Using uninitialized value! Fuzzy value for function: " + _name + " was not set");

                return (_fuzzy);
            }
            set { _fuzzy = value; }
        }

        #endregion

        #region Constructor

        public Function()
        {
            _name = "New function";
            _begin = new FKey(Vector2.zero);
            _end = new FKey(Vector2.zero);

            _function = new AnimationCurve(new Keyframe[] {_begin, _end });
        }
        public Function(FunctionConfig config)
        {
            _name = "New function";
            _begin = config.Func.Begin;
            _end = config.Func.End;
            _function = config.Func;
        }

        #endregion

        #region Core functions

        public virtual float Evaluate(float x)
        {
            return (_function.Evaluate(x));
        }

        public void UpdateAnimationCurveValues()
        {
            Keyframe[] __frames = _function.keys;
            __frames[0].time = _begin.X;
            __frames[0].value = _begin.Y;

            __frames[_function.keys.Length - 1].time = _end.X;
            __frames[_function.keys.Length - 1].value = _end.Y;

            _function.keys = __frames;
        }

        #endregion

        #region Utility

        protected bool CompareCurveKeyframes(Function f)
        {
            if (_function.length != f._function.length)
                return false;

            for (int i = 0; i < _function.length; i++)
            {
                for (int j = 0; j < f._function.length; j++)
                {
                    if (!(_function[i].time == f._function[i].time &&
                        _function[i].value == f._function[i].value))
                    {
                        return (false);
                    }
                }
            }

            return (true);
        }

        #endregion
    }

    [System.Serializable]
    public class ConditionExpression
    {
        [SerializeField] private Concept _concept;
        [SerializeField] private Function _function;
        [SerializeField] private Operator _operator;

        public Concept Concept { get { return _concept; } set { _concept = value; } }
        public Function Function { get { return _function; } set { _function = value; } }
        public Operator Operator { get { return _operator; } set { _operator = value; } }

        public ConditionExpression()
        {

        }
        public ConditionExpression(Concept c, Function f, Operator op)
        {
            Concept = c;
            Function = f;
            Operator = op;
        }
    }

    [System.Serializable]
    public class Condition
    {
        [SerializeField] protected List<ConditionExpression> _expressionList;

        public Concept TargetConcept
        {
            get
            {
                if (_expressionList == null || _expressionList.Count <= 0)
                    return (null);

                return (_expressionList[0].Concept);
            }
        }
        public Function TargetFunction
        {
            get
            {
                if (_expressionList == null || _expressionList.Count <= 0)
                    return (null);

                return (_expressionList[0].Function);
            }
        }
        public List<ConditionExpression> ExpressionList
        {
            get { return (_expressionList); }
        }

        public Condition()
        {
            if(_expressionList == null)
                _expressionList = new List<ConditionExpression>();
        }
        public Condition(Concept c, Function f, Operator op)
        {
            if (_expressionList == null)
                _expressionList = new List<ConditionExpression>();

            _expressionList.Add(new ConditionExpression(c, f, op));
        }

        public int AddExpression(ConditionExpression expression)
        {
            if (ExpressionList != null)
                ExpressionList.Add(expression);

            return (ExpressionList.IndexOf(expression));
        }

        public void RemoveExpression(ConditionExpression expression)
        {
            if(ExpressionList != null && ExpressionList.Contains(expression))
            {
                ExpressionList.Remove(expression);
            }
        }
        public void RemoveExpression(int expressionIndex)
        {
            if(ExpressionList != null && expressionIndex < ExpressionList.Count)
            {
                ExpressionList.RemoveAt(expressionIndex);
            }
        }
        public void RemoveExpression()
        {
            if(ExpressionList != null && ExpressionList.Count > 0)
            {
                ExpressionList.RemoveAt(ExpressionList.Count - 1);
            }
        }

        public void ExecuteCondition()
        {
            Operator __currentOperator = Operator.EQUALS;
            float __currentValue = 0;

            for (int i = 1; i < ExpressionList.Count; i++)
            {
                switch(__currentOperator)
                {
                    case Operator.EQUALS:
                        {
                            __currentValue = ExpressionList[i].Concept.IS(ExpressionList[i].Function.Name);
                        }
                        break;

                    case Operator.AND:
                        {
                            __currentValue = Logic.AND(__currentValue, ExpressionList[i].Concept.IS(ExpressionList[i].Function.Name));
                        }
                        break;

                    case Operator.OR:
                        {
                            __currentValue = Logic.OR(__currentValue, ExpressionList[i].Concept.IS(ExpressionList[i].Function.Name));
                        }
                        break;

                    case Operator.END:
                        {
                            
                        }
                        break;
                }

                __currentOperator = ExpressionList[i].Operator;
            }

            _expressionList[0].Concept.GetFunction(_expressionList[0].Function.Name).Fuzzy = __currentValue;
        }
    }
}