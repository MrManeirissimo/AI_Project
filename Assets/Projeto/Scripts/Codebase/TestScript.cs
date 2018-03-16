using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    Fuzzy.FuzzyBehaviour _fBehaviour;
    public float distancia;
    public float vida;
    public float municao;
    public float inimigos;

    private void FuzzyCalculations()
    {
        _fBehaviour.SetConceptValue("Distancia", distancia);
        _fBehaviour.SetConceptValue("Vida", vida);
        _fBehaviour.SetConceptValue("Municao", municao);
        _fBehaviour.SetConceptValue("Inimigos", inimigos);

        _fBehaviour.ExecuteConditions();

        Fuzzy.Concept __risco = _fBehaviour.GetConcept("Risco");
        for (int i = 0; i < _fBehaviour.GetConcept("Risco").Functions.Count; i++)
        {
            print("Concept " + __risco.Functions[i].Name + ": " + __risco.Functions[i].Fuzzy);
        }

        print("Defuzzy: " + __risco.Defuzzyfication(0.001f));
    }
    
    private IEnumerator UpdateFuzzy(float interval)
    {
        while(true)
        {
            yield return new WaitForSeconds(interval);
            FuzzyCalculations();
        }
    }

    private void Awake()
    {
        _fBehaviour = GetComponent<Fuzzy.FuzzyBehaviour>();
    }

    void Start ()
    {
        StartCoroutine(UpdateFuzzy(5.0f));
	}
}
