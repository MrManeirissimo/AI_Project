using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using IA_Project.Utility.Classes;

public class Controller : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] protected SimulationConfiguration _systemConfig;

    [Header("References")]
    [SerializeField] protected MapDarwin _genetic;
    [SerializeField] protected BotMap _botMap;
    [SerializeField] protected InterfaceDisplay _interface;

    [Header("Options")]
    [SerializeField] protected bool _isPaused = false;

    public event System.Action OnAlgorithmStart;

    private void Awake()
    {
        if (!_systemConfig) _systemConfig = (SimulationConfiguration)Resources.Load("Configs\\Darwing_config");
    }
    private void OnValidate()
    {
        if (!_genetic) _genetic = FindObjectOfType<MapDarwin>();
        if (_botMap) _botMap = FindObjectOfType<BotMap>();
        if (!_interface) _interface = FindObjectOfType<InterfaceDisplay>();
    }
    private void OnEnable()
    {
        if(_genetic)
        {
            _genetic.AgentGenerated += AddAgentToDisplayQueue;

            _genetic.AgentGenerated += _interface.UI_UpdateAgentGenerated;

            _genetic.FitnessUpdated += _interface.UI_UpdateAvgFitness;
        }
    }
    private void Start()
    {
        Application.runInBackground = true;

        InitializeCamps();
    }

    private void InitializeCamps()
    {
        _interface.InFields[0].text = _systemConfig.MapSize.ToString();
        _interface.InFields[1].text = _systemConfig.InitialPopulation.ToString();
        _interface.InFields[2].text = _systemConfig.DesiredFitness.ToString();
        _interface.InFields[3].text = _systemConfig.ProcreationStep.ToString();
        _interface.InFields[4].text = _systemConfig.DisplayStep.ToString();

        _botMap.DisplayFittestOnly = _interface.FittestOnly.isOn = _systemConfig.DisplayFittestOnly;
    }
    private void AddAgentToDisplayQueue(List<List<TileData>> agent)
    {
        if (!_botMap.DisplayFittestOnly)
        {
            _botMap.AddToDrawQueue(agent);
            return;
        }

        if (agent == _genetic.Fittest)
            _botMap.AddToDrawQueue(agent);
    }

    #region Public button functions

    public void RunGeneticAlgorithm()
    {
        if(!_isPaused)
        {
            _botMap.ConstructMap();
            _botMap.BeginDisplayLoop();
            _genetic.RunAlgorithm();

            if (OnAlgorithmStart != null)
                OnAlgorithmStart();
        }

        else
        {
            _genetic.ResumeAlgorithm();
            _botMap.SetTimerState(true);

            _interface.TogglePauseOverlay(false);
            _isPaused = false;
        }
    }

    public void ToggleDisplayOnlyFittest(Toggle value)
    {
        _systemConfig.DisplayFittestOnly = _botMap.DisplayFittestOnly = value.isOn;
    }

    public void ChangeMapSize(InputField field)
    {
        if(!string.IsNullOrEmpty(field.text))
        {
            int __value = int.Parse(field.text);
            _systemConfig.MapSize = _botMap.MapSize = __value;
        }
        else
        {
            _botMap.MapSize = 0;
        }
    }

    public void ChangeDesiredFitness(InputField field)
    {
        if (!string.IsNullOrEmpty(field.text))
        {
            int __value = int.Parse(field.text);
            _systemConfig.DesiredFitness = _genetic.DesiredFitness = __value;
        }
        else
        {
            _genetic.DesiredFitness = 0;
        }
    }

    public void ChangeInitialPopulation(InputField field)
    {
        if (!string.IsNullOrEmpty(field.text))
        {
            int __value = int.Parse(field.text);
            _systemConfig.InitialPopulation = _genetic.InitialPopulation = __value;
            _interface.SetTournamentSliderMaxValue(__value);
        }
        else
        {
            _genetic.InitialPopulation = 0;
        }
    }

    public void ChangeProcreationStep(InputField field)
    {
        if(_genetic)
        {
            if(!string.IsNullOrEmpty(field.text))
            {
                float __value = -1;
                float.TryParse(field.text, out __value);

                if(__value >= 0)
                    _systemConfig.ProcreationStep = _genetic.ProcreationStep = __value;
            }
            else
            {
                _systemConfig.ProcreationStep = _genetic.ProcreationStep = 0;
            }
        }
    }

    public void ChangeDisplayStep(InputField field)
    {
        if (_botMap)
        {
            if (!string.IsNullOrEmpty(field.text))
            {
                float __value = -1;
                float.TryParse(field.text, out __value);

                if (__value >= 0)
                    _systemConfig.DisplayStep = _botMap.DisplayInterval = __value;
            }
            else
            {
                _systemConfig.DisplayStep = _botMap.DisplayInterval = 0;
            }
        }
    }

    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void PauseSim()
    {
        if(!_isPaused)
        {
            _isPaused = true;
            _interface.TogglePauseOverlay(true);

            _botMap.SetTimerState(false);
            _genetic.PauseAlgorithm();
        }
    }

    public void CloseAplication()
    {
        Application.Quit();
    }

    public void SelectCrossovertype(Dropdown selection)
    {
        if(selection.value == 0) _genetic.MethodOfCrossover = MapDarwin.CrossoverMethod.One_point;

        else _genetic.MethodOfCrossover = MapDarwin.CrossoverMethod.Two_point;   
    }

    public void SelectSelectionMethod(Dropdown selection)
    {
        if (!selection)
            return;

        if (selection.value >= System.Enum.GetNames(typeof(MapDarwin.SelectionMethod)).Length)
            throw new System.Exception("Out of bounds");

        _genetic.MethodOfSelection = (MapDarwin.SelectionMethod)selection.value;
    }

    public void SetTournamentSize(Slider slider)
    {
        if(slider)
        {
            if(_genetic.MethodOfSelection == MapDarwin.SelectionMethod.Tournament)
                _genetic.TournamentSize = (int)slider.value;
        }
    }

    public void SetTruncationCut(Slider slider)
    {
        if (slider)
        {
            if (_genetic.MethodOfSelection == MapDarwin.SelectionMethod.Truncation)
                _genetic.TruncationAmmount = slider.value / slider.maxValue;
        }
    }

    #endregion
}
