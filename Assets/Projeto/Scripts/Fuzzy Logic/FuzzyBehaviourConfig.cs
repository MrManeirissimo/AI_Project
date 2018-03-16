using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fuzzy
{
    public class FuzzyBehaviourConfig : ScriptableObject
    {
        [SerializeField] protected FuzzyBehaviour _behaviour;
        [SerializeField] protected List<Color> _colorList;

        public List<Color> ColorList
        {
            get
            {
                if (_colorList == null)
                    _colorList = new List<Color>();

                return (_colorList);
            }
            set { _colorList = value; }
        }

        public FuzzyBehaviour Behaviour
        {
            get { return _behaviour; }
            set { _behaviour = value; }
        }
        public void SetBehaviour(FuzzyBehaviour behaviour)
        {
            this._behaviour = behaviour;
        }

        public void GetNewColor()
        {
            ColorList.Add(Random.ColorHSV(0, 1, 0.2f, 0.3f, 0.7f, 1.0f, 0.8f, 1.0f));
        }
        public void GenerateNewColorForIndex(int index)
        {
            if (index < 0 || index > ColorList.Count)
                throw new System.Exception("@FuzzyBehaviourConfig - index out of bounds, cannot generate new color for index: " + index);

            ColorList[index] = Random.ColorHSV(0, 1, 0.2f, 0.3f, 0.7f, 1.0f, 0.8f, 1.0f);
        }
    }
}