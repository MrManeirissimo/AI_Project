using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using IA_Project.Utility.Classes;

public class InterfaceDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapDarwin _darwin;
    [SerializeField] private BotMap _mapDisplayer;
    [SerializeField] private Transform _overlay;

    [Header("TextFields")]
    [SerializeField] private Text _agnt_id;
    [SerializeField] private Text _angt_generated;
    [SerializeField] private Text _gen;
    [SerializeField] private Text _timer;
    [SerializeField] private Text _avgFitness;
    [SerializeField] private Text _targetFitness;

    [Header("InputFields")]
    [SerializeField] private InputField _mapSize;
    [SerializeField] private InputField _initialPop;
    [SerializeField] private InputField _desiredFitness;
    [SerializeField] private InputField _procStep;
    [SerializeField] private InputField _displayStep;

    [Header("Checboxes")]
    [SerializeField] private Toggle _fittestOnly;

    [Header("Sliders")]
    [SerializeField] private Slider _sliderTournament;

    [Header("Transforms")]
    [SerializeField] private Transform _optionsTab;

    [Header("Values")]
    [SerializeField] private bool _tabRaised;

    public InputField[] InFields
    {
        get { return new InputField[5] { _mapSize, _initialPop, _desiredFitness, _procStep, _displayStep }; }
    }
    public Toggle FittestOnly
    {
        get { return _fittestOnly; }
        set { _fittestOnly = value; }
    }

    public void UI_UpdateAvgFitness(float fitness)
    {
        _avgFitness.text = "Avg.Fitness: " + fitness;
    }
    public void UI_UpdateAgentInfo(List<List<TileData>> map)
    {
        _agnt_id.text = "Agent : " + (map as MapDarwin.Agent).id + " (display)";
    }
    public void UI_UpdateAgentGenerated(List<List<TileData>> map)
    {
        MapDarwin.Agent __agent = (MapDarwin.Agent)map;
        _angt_generated.text = "Agent : " + __agent.id + " (generated)";
    }
    public void TogglePauseOverlay(bool value)
    {
        _overlay.gameObject.SetActive(value);
    }
    private void UpdateTargetFitness(MapDarwin.Agent agent)
    {
        if (_targetFitness) _targetFitness.text = "Target fitness: " + agent.fitness.ToString();
    }

    private void Awake()
    {
        if (!_darwin) _darwin = FindObjectOfType<MapDarwin>();
        if (!_mapDisplayer) _mapDisplayer = FindObjectOfType<BotMap>();
    }
    private void OnEnable()
    {
        if(_mapDisplayer) _mapDisplayer.OnDrawAgent += UI_UpdateAgentInfo;
        if(_darwin) _darwin.AgentGenerated += UpdateTargetFitness;
    }

    

    private void Update()
    {
        _timer.text = Time.time.ToString("00");
    }

    public void ToggleTab()
    {
        if(_tabRaised)
        {
            _optionsTab.GetComponent<Animator>().SetTrigger("lower");
        }
        else
        {
            _optionsTab.GetComponent<Animator>().SetTrigger("raise");
        }

        _tabRaised = !_tabRaised;
    }

    public void SetTournamentSliderMaxValue(int max)
    {
        _sliderTournament.maxValue = max;
    }
}
