using System.Collections.Generic;
using UnityEngine;

namespace Fuzzy
{
    [CreateAssetMenu(menuName = "Fuzzy/Concept")]
    public class Concept : ScriptableObject
    {
        [Header("Concept name")]
        [SerializeField] protected string _name;

        [Header("Concept value")]
        [SerializeField] protected float _value;

        [Header("Function List")]
        [SerializeField] protected List<Function> _functions;

        #region Accessors

        public float Value
        {
            get { return (_value); }
            set { _value = value; }
        }
        public List<Function> Functions
        {
            get
            {
                if (_functions == null)
                    _functions = new List<Function>();

                return (_functions);
            }
        }

        #endregion

        public Function AddFunction(FunctionConfig func)
        {
            Functions.Add(func);

            return (func);
        }
        public Function AddFunction()
        {
            Function func = new Function();
            Functions.Add(func);

            return (func);
        }

        public Function RemoveFunction(string functionName)
        {
            Function __func = null;
            if(_functions != null && _functions.Count > 0)
            {
                for (int i = 0; i < _functions.Count; i++)
                {
                    if (functionName == _functions[i].Name)
                    {
                        __func = _functions[i];
                        _functions.RemoveAt(i);
                        break;
                    }
                }
            }            

            return (__func);
        }

        public Function GetFunction(Function func)
        {
            foreach (var f in _functions)
            {
                if (f.Equals(func))
                {
                    return(f);
                }
            }

            return (null);
        }
        public Function GetFunction(string funcName)
        {
            foreach (var f in _functions)
            {
                if (f.Name == funcName)
                {
                    return (f);
                }
            }

            return (null);
        }
        public Function GetFunction(int funcIndex)
        {
            if (funcIndex >= _functions.Count)
                throw new System.Exception("@Fuzzy exception - Could not find function of index " + funcIndex);

            return (_functions[funcIndex]);
        }
        public List<string> GetFuctionNames()
        {
            if (_functions.Count <= 0)
                return null;

            List<string> __functionNames = new List<string>();
            foreach (var function in _functions)
            {
                __functionNames.Add(function.Name);
            }

            return (__functionNames);
        }

        public float Fuzzyfication(float val, string funcName)
        {
            Function func = GetFunction(funcName);
            if (!func)
            {
                throw new System.Exception("@Fuzzy Exception - Function not found");
            }

            return (func.Evaluate(val));
        }
        public float Defuzzyfication(float precision)
        {
            float __begin = 0;
            float __end = 0;
            bool __first = true;

            foreach (var func in _functions)
            {
                if (__first)
                {
                    __begin = func.Begin.X;
                    __end = func.End.X;
                }
                else
                {
                    __begin = (func.Begin.X < __begin ? func.Begin.X : __begin);
                    __end = (func.End.X < __end ? func.End.X : __end);
                }
            }

            float __area = -0.5f;
            float __sum = 0.0f;

            for (float i = __begin; i < __end; i += precision)
            {
                float __value = 0;
                foreach (var func in _functions)
                {
                    __value = Logic.MAX(__value, Logic.MIN(func.Evaluate(i), func.Fuzzy));
                }

                __area += (__value * i);
                __sum += __value;
            }

            return (__area / __sum);
        }
        public float IS(string concept)
        {
            Function f = GetFunction(concept);
            if (!f)
            {
                throw new System.Exception("@Fuzzy Exception - Function not found or no function assigned");
            }

            return (f.Evaluate(_value));
        }
        public float IS(Function function)
        {
            if(!Functions.Contains(function))
            {
                throw new System.Exception("@Fuzzy Exception - Function not found or no function assigned");
            }

            return (function.Evaluate(_value));
        }

        public void UpdateAllFunctionValues()
        {
            if (_functions == null)
                return;

            foreach (var func in _functions)
            {
                func.UpdateAnimationCurveValues();
            }
        }
        public void RestFunctionValues()
        {
            foreach (var func in _functions)
            {
                func.Fuzzy = -1;
            }
        }
        public void ResetFuzzyValue()
        {
            _value = -1;
        }
    }
}