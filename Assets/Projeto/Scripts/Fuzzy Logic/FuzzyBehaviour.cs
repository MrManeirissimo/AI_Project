using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fuzzy
{
    public class FuzzyBehaviour : MonoBehaviour
    {
        [SerializeField] protected FuzzyBehaviourConfig _config;
        [SerializeField] protected List<Concept> _relatedConcepts;
        [SerializeField] protected List<Condition> _conditionList;

        public FuzzyBehaviourConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public float PrecisionValue = 0.01f;

        public List<Concept> ConceptList
        {
            get
            {
                if (_relatedConcepts == null)
                    _relatedConcepts = new List<Concept>();

                return (_relatedConcepts);
            }
            set
            {
                _relatedConcepts = value;
            }
        }
        public List<Condition> ConditionList
        {
            get
            {
                if (_conditionList == null)
                    _conditionList = new List<Condition>();

                return (_conditionList);
            }
            set
            {
                _conditionList = value;
            }
        }

        public Condition CreateCondition()
        {
            Condition __newCondition = new Condition();

            ConditionList.Add(__newCondition);
            return (__newCondition);
        }
        public Condition CreateCondition(Concept c, Function f, Operator op)
        {
            Condition __newCondition = new Condition(c, f, op);

            ConditionList.Add(__newCondition);

            return (__newCondition);
        }

        public void RemoveCondition()
        {
            if (ConditionList.Count < 0)
                return;

            ConditionList.RemoveAt(ConditionList.Count - 1);
        }
        public void RemoveAllConditions()
        {
            ConditionList.Clear();
        }

        public void RemoveConcept(int index)
        {
            if (index < 0 || index >= ConceptList.Count)
                throw new System.Exception("@Fuzzy behaviour - index out of bounds in concepList. Cannot remove concept of index: " + index);

            ConceptList.RemoveAt(index);
        }
        public void RemoveConcept(string name)
        {
            foreach (var concept in ConceptList)
            {
                if (concept.name == name)
                {
                    ConceptList.Remove(concept);
                    return;
                }
            }

            throw new System.Exception("@FuzzyBehaviour -  could not find concept of name: " + name + ". Removal failed");
        }

        public void AddConcept(Concept c)
        {
            ConceptList.Add(c);
        }

        public bool ContainsConcept(Concept c)
        {
            return (ConceptList.Contains(c));
        }
        public bool ContainsConcept(string name)
        {
            foreach (var concept in ConceptList)
            {
                if (concept.name == name)
                    return (true);
            }

            return (false);
        }

        public float DefuzzyConcept(Concept concept)
        {
            if(!ConceptList.Contains(concept))
            {
                throw new System.Exception("@Fuzzy Exception - No concept of name: " + concept.name + "found inside related concepts");
            }

            return(concept.Defuzzyfication(PrecisionValue));
        }
        public float DefuzzyConcept(string name)
        {
            foreach (var concept in ConceptList)
            {
                if(concept.name == name)
                {
                    return (concept.Defuzzyfication(PrecisionValue));
                }
            }

            return (default(float));
        }

        public bool SetConceptValue(int index, float value)
        {
            if(index < ConceptList.Count && index >= 0)
            {
                if(ConceptList[index].Value != value)
                {
                    ConceptList[index].Value = value;
                    return (true);
                }
            }

            return (false);
        }
        public bool SetConceptValue(string name, float value)
        {
            foreach (var concept in ConceptList)
            {
                if(concept.name == name)
                {
                    concept.Value = value;
                    return (true);
                }
            }

            return (false);
        }
        public bool SetConceptValue(Concept concept, float value)
        {
            try
            {
                ConceptList[ConceptList.IndexOf(concept)].Value = value;
                return (true);
            }
            catch
            {
                return (false);
            }
        }

        public Concept GetConcept(string name)
        {
            foreach (var concept in ConceptList)
            {
                if (concept.name == name)
                    return (concept);
            }

            return (null);
        }
        public Concept GetConcept(int index)
        {
            return (ConceptList[index]);
        }

        public void ExecuteConditions()
        {
            foreach (var condition in ConditionList)
            {
                condition.ExecuteCondition();
            }
        }
        public void ExecuteSingleCondition(int conditionIndex)
        {
            if (conditionIndex < 0 || conditionIndex >= ConditionList.Count)
            {
                throw new System.Exception("@FuzzyBehaviour - Cannot resolve condition, index: " + conditionIndex +
                    " is out of bound from condition list");
            }

            ConditionList[conditionIndex].ExecuteCondition();
        }

        public void Awake()
        {
            foreach (var concept in ConceptList)
            {
                concept.ResetFuzzyValue();
                concept.RestFunctionValues();
            }
        }
        //public void Start()
        //{
        //    Concept c;
        //    c = GetConcept("Vida");
        //    if(c) c.Value = 8;

        //    c = GetConcept("Distancia");
        //    if (c) c.Value = 3;

        //    c = GetConcept("Inimigos");
        //    if (c) c.Value = 8;

        //    c = GetConcept("Municao");
        //    if (c) c.Value = 7;


        //    float distanciaPerto = GetConcept("Distancia").GetFunction("Muito Perto").Evaluate(3);
        //    float vidaCritica = GetConcept("Vida").GetFunction("Critica").Evaluate(8);
        //    print("Distanca muito perto: " + distanciaPerto);
        //    print("Vida critica: " + vidaCritica);

        //    float value = Logic.OR(distanciaPerto, vidaCritica);
        //    print("Risco pequeno: " + value);
        //}
        //public void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.KeypadEnter))
        //    {
        //        ExecuteConditions();

        //        Debug.Log("Condition executed");
        //        Concept __risco = GetConcept("Risco");
        //        print("Pequeno: " + __risco.GetFunction("Pequeno").Fuzzy);
        //        print("Medio: " + __risco.GetFunction("Medio").Fuzzy);
        //        print("Alto: " + __risco.GetFunction("Alto").Fuzzy);

        //        //Function pequeno = __risco.GetFunction("Pequeno");
        //        //print("Test1: " + __risco.IS("Pequeno"));
        //        //print("Test2: " + __risco.IS(pequeno));
        //    }
        //}
    }
}