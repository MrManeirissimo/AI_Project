using UnityEngine;

namespace Fuzzy
{
    [CreateAssetMenu(menuName = "Fuzzy/Function config")]
    public class FunctionConfig : ScriptableObject
    {
        [SerializeField] protected Function function;

        public Function Func
        {
            get { return (function); }
            set { function = value; }
        }

        public static implicit operator Function(FunctionConfig config)
        {
            return (config.Func);
        }
    }
}