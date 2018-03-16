using UnityEngine;

[CreateAssetMenu(menuName = "Sim Configuration/Config")]
public class SimulationConfiguration : ScriptableObject
{
    [SerializeField] public int MapSize;
    [SerializeField] public int InitialPopulation;
    [SerializeField] public int DesiredFitness;
    [SerializeField] public float ProcreationStep;
    [SerializeField] public float DisplayStep;
    [SerializeField] public MapDarwin.SelectionMethod SelectionMethod;
    [SerializeField] public MapDarwin.CrossoverMethod CrossoverMethod;

    [SerializeField] public bool DisplayFittestOnly = false;
}
