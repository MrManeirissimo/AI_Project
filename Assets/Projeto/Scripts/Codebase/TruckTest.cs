using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fuzzy;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FuzzyBehaviour))]
public class TruckTest : MonoBehaviour
{
    [Header("References")]
    private FuzzyBehaviour _fBehaviour;
    private Rigidbody _body;

    public GameObject stopRef;


    [Header("Values")]
    public float velocidade;
    public float distancia;
    public float acceleracao;

    private void FuzzyCalculations()
    {
        _fBehaviour.SetConceptValue("Truck_Distancia", distancia);
        _fBehaviour.SetConceptValue("Truck_Velocidade", velocidade);


        Concept __concept =  _fBehaviour.GetConcept("Truck_Risco");
        _fBehaviour.ExecuteConditions();
        print("Risco 1: " + __concept.GetFunction(0).Fuzzy);
        print("Risco 2: " + __concept.GetFunction(1).Fuzzy);
        print("Risco 3: " + __concept.GetFunction(2).Fuzzy);

        print("Defuzzy: " + __concept.Defuzzyfication(0.001f));
        Debug.Break();
    }

    private IEnumerator UpdateFuzzy(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            FuzzyCalculations();
        }
    }

    private void Awake()
    {
        _fBehaviour = GetComponent<Fuzzy.FuzzyBehaviour>();
        _body = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        StartCoroutine(UpdateFuzzy(.2f));
    }
    private void FixedUpdate()
    {
        _body.AddForce(transform.forward * acceleracao * Time.fixedDeltaTime, ForceMode.Acceleration);
    }
    private void Update()
    {
        velocidade = _body.velocity.magnitude;
        distancia = Vector3.Distance(stopRef.transform.position, transform.position);
    }
}
