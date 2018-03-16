using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MindTrick;

using IA_Project.Utility.Classes;
using IA_Project.Utility.Enumerators;

public class MapDarwin : MonoBehaviour
{
    delegate Agent[] CrossoverCallback(Agent mom, Agent dad);
    delegate Agent[] SelectionCallback();

    public enum CrossoverMethod
    {
        One_point,
        Two_point
    }
    public enum SelectionMethod
    {
        Proportional,
        Tournament,
        Truncation,
        LinearLeveling
    }

    #region Nested Classes

    public class Agent : List<List<TileData>>, System.IComparable<Agent>
    {
        public int fitness = 0;
        public uint generation;
        public string id = string.Empty;

        //Ensures that by constructing an agent the default constructor for List<List<TileType> is called
        public Agent(IEnumerable<List<TileData>> data) : base(data)
        {

        }

        public int CompareTo(Agent other)
        {
            return (fitness.CompareTo(other.fitness));
        }
    }

    [System.Serializable] public class SelectionPair
    {
        int start, range;
        [SerializeField] int first, second;

        public event System.Action PairReseted;

        public int First
        {
            get { return (first); }
        }
        public int Second
        {
            get { return (second); }
        }
        public int Start
        {
            get { return (start); }
        }
        public int Range
        {
            get { return (range); }
        }

        public SelectionPair(int start, int range)
        {
            this.start = start;
            this.range = range;

            first = start;
            second = start + 1;
        }
        public void Reset()
        {
            first = start;
            second = first + 1;

            if (PairReseted != null)
                PairReseted();
        }

        public static SelectionPair operator ++(SelectionPair pair)
        {
            pair.first++; pair.second++;

            if (pair.second > pair.range)
                pair.Reset();

            return (pair);
        }
    }

    #endregion

    #region Constant Fields
    private static uint _agntIndex = 0;
    private static int _defaultQuickScore = 10;
    #endregion

    #region Private Fields
    private List<Agent> _population = new List<Agent>();
    private Agent _bestAgent = null;

    [Header("References")]
    [SerializeField] private BotMap _generatorTempPlsChange;
    [SerializeField] private SelectionPair _procreationIndex;

    [Header("Values")]
    [SerializeField] [Range(0, 1)] private float _mutationRate = 0.05f;
    [SerializeField] private int _initialPopulation = 100;
    [SerializeField] private int _desiredFitness = 400;
    [SerializeField] private float _procreationStep = 1.0f;

    [SerializeField] private float _avgFitness = 0.0f;

    [Header("Options")]
    [SerializeField] private SelectionMethod _selectionMethod;
    [SerializeField] private CrossoverMethod _crossoverMethod;

    [Header("Tournaments selection")]
    [SerializeField] private int _tournamentSize = 10;

    [Header("Truncation selection")]
    [SerializeField] [Range(0, 1)] private float _truncationAmmount = 0.5f;


    [Header("Timers")]
    [SerializeField] private Timer _tProcreation;

    //Callback variables
    private CrossoverCallback _crossoverCallback;
    private SelectionCallback _selectionCallback;

    #endregion

    #region Public Events
    public delegate void AgentDelgate(Agent a);
    public delegate void FitDelgate(float a);

    public event FitDelgate FitnessUpdated;
    public event AgentDelgate FittestChanged;
    public event AgentDelgate AgentGenerated;
    #endregion

    #region Public properties

    public List<List<TileData>> Fittest
    {
        get { return (_bestAgent); }
    }
    public BotMap Generator
    {
        get { return _generatorTempPlsChange; }
        set { _generatorTempPlsChange = value; }
    }
    public int DesiredFitness
    {
        get { return (_desiredFitness); }
        set { _desiredFitness = value; }
    }
    public int InitialPopulation
    {
        get { return _initialPopulation; }
        set { _initialPopulation = value; }
    }
    public uint CurrentAgentIndex
    {
        get { return (_agntIndex); }
    }
    public float ProcreationStep
    {
        get { return _procreationStep; }
        set { _procreationStep = value; }
    }
    public float AvgFitness
    {
        get { return _avgFitness; }
    }

    public CrossoverMethod MethodOfCrossover
    {
        get { return _crossoverMethod; }
        set
        {
            if (value == CrossoverMethod.One_point)
                _crossoverCallback = OnePointCrossover;

            else
                _crossoverCallback = TwoPointCrossover;

            _crossoverMethod = value;
        }
    }

    public SelectionMethod MethodOfSelection
    {
        get { return _selectionMethod; }
        set
        {
            _selectionMethod = value;

            switch(_selectionMethod)
            {
                case SelectionMethod.Proportional:
                    {
                        _selectionCallback = ProportionalSelection;
                    }
                    break;

                case SelectionMethod.LinearLeveling:
                    {
                        
                    }
                    break;

                case SelectionMethod.Tournament:
                    {
                        _selectionCallback = TournamentSelection;
                    }
                    break;

                case SelectionMethod.Truncation:
                    {
                        _selectionCallback = TruncationSelection;
                    }
                    break;
            }
        }
    }

    public int TournamentSize { get { return _tournamentSize;} set { _tournamentSize = value; } }
    public float TruncationAmmount { get { return _truncationAmmount; } set { _truncationAmmount = value; } }

#endregion

    #region Core methods

    //Creation

    /// <summary>
    /// Generates a random Agent with random map distribution from the generator
    /// </summary>
    /// <returns></returns>
    private Agent GetChromosomePool()
    {
        return (new Agent(_generatorTempPlsChange.GenerateMapData()));
    }

    /// <summary>
    /// Creates a new Agent from scratch. Generates a random Agent with random map distribution from the generator
    /// </summary>
    private Agent Autogenesis()
    {
        Agent __agent = GetChromosomePool();

        return (__agent);
    }

    /// <summary>
    /// Deprecated
    /// </summary>
    /// <param name="amount"></param>
    private void CreatePopulation(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Agent __agent = Autogenesis();
            RegistrationAdnEvaluation(ref __agent);
        }
    }

    //Registration & Evaluation

    /// <summary>
    /// Registers the agent, giving him an Id and adding it to the population list.
    /// Also evaluates the agent's chromosome pool (tile information).
    /// Starts the AgentGenerated event
    /// </summary>
    /// <param name="agent"></param>
    private void RegistrationAdnEvaluation(ref Agent agent)
    {
        Prosthesize(ref agent);
        Evaluate(ref agent);

        _avgFitness = CalculateAvgFitness();

        if (FitnessUpdated != null)
            FitnessUpdated(_avgFitness);

        if (AgentGenerated != null)
            AgentGenerated(agent);
    }

    /// <summary>
    /// Registration process
    /// </summary>
    private void Prosthesize(ref Agent agent)
    {
        _agntIndex++;

        uint __id = _agntIndex;

        agent.id = "_agnt_" + __id;
        _population.Add(agent);
    }

    /// <summary>
    /// Main evaluation process. Calls all other evaluation functions
    /// </summary>
    /// <param name="agent"></param>
    private void Evaluate(ref Agent agent)
    {
        int __fitness = 0;

        __fitness = ReEvaluateAdjacency(ref agent);

        agent.fitness = __fitness;

        if (_bestAgent == null || (_bestAgent != null && _bestAgent.fitness < __fitness))
        {
            _bestAgent = agent;

            if (FittestChanged != null)
                FittestChanged(_bestAgent);
        }

        if (__fitness >= _desiredFitness)
            _tProcreation.Cancel();
    }

    private void EvaluateTileConnection(ref Agent agent)
    {
        for (int i = 0; i < agent.Count; i++)
        {
            for (int j = 0; j < agent[i].Count; j++)
            {

            }
        }
    }

    //Procreation
    private Agent Mutate(Agent agent, int ammount)
    {
        for (int i = 0; i < ammount; i++)
        {
            int __indexX = Random.Range(0, agent.Count - 1);
            int __indexY = Random.Range(0, agent.Count - 1);

            agent[__indexX][__indexY] = _generatorTempPlsChange.GetRandomTile();
        }

        return (agent);
    }
    private Agent[] TwoPointCrossover(Agent mom, Agent dad)
    {
        // Creating list of children
        List<Agent> __children = new List<Agent>();
        __children.Add(new Agent(new List<List<TileData>>()));
        __children.Add(new Agent(new List<List<TileData>>()));

        //Generates random line of cut for the chromosome
        int __splitY = Random.Range(1, _generatorTempPlsChange.MapSize - 2);
        int __splitX = Random.Range(1, _generatorTempPlsChange.MapSize - 2);

        int size = mom.Count == dad.Count ? mom.Count : 0;
        if (size == 0)
            throw new System.Exception("@MapDarwin - Mom and Dad do not have same number of cromossomes (TileData)");

        for (int i = 0; i < size; i++)
        {
            if(i < __splitY || i > __splitX)
            {
                __children[0].Add(mom[i]);
                __children[1].Add(dad[i]);
            }
            else
            {
                __children[0].Add(dad[i]);
                __children[1].Add(mom[i]);
            }
        }

        return (__children.ToArray());
    }
    private Agent[] OnePointCrossover(Agent mom, Agent dad)
    {
        //Creating list of children
        List<Agent> __children = new List<Agent>();
        __children.Add(new Agent(new List<List<TileData>>()));
        __children.Add(new Agent(new List<List<TileData>>()));

        //Generates random line of cut for the chromosome
        int __splitY = Random.Range(1, _generatorTempPlsChange.MapSize - 2);

        int size = mom.Count == dad.Count ? mom.Count : 0;
        if (size == 0)
            throw new System.Exception("@MapDarwin - Mom and Dad do not have same number of cromossomes (TileData)");

        //If bigger half of the split: 1 -> child1, 2 -> child2 : 1 -> child2, 2 -> child1
        for (int i = 0; i < size; i++)
        {
            if(i <= __splitY)
            {
                __children[0].Add(mom[i]);
                __children[1].Add(dad[i]);
            }
            else
            {
                __children[0].Add(dad[i]);
                __children[1].Add(mom[i]);
            }
        }

        return (__children.ToArray());
    }

    /// <summary>
    /// Crosses the genes of the fittest half of the population
    /// </summary>
    private void Procreate()
    {
        //Assignment of parent which will get to procreate
        Agent[] __parents = _selectionCallback();
        if(__parents.Length > 2 || __parents.Length < 2 )
        {
            throw new System.Exception("@MapDarwin - Invalid number of parents");
        }

        //Generation and crossover
        Agent[] __offspring = _crossoverCallback(__parents[0], __parents[1]);

        //Mutation part
        int __mutationAmount = Mathf.CeilToInt(_generatorTempPlsChange.MapSize * _mutationRate);
        for (int i = 0; i < __offspring.Length; i++)
        {
            //Mutates each child
            __offspring[i] = Mutate(__offspring[i], __mutationAmount);

            //Evaluates and registers
            RegistrationAdnEvaluation(ref __offspring[i]);
        }

        _procreationIndex++;

        //Removal part. We sort in order of fitness, the lowest are discarded
        SortPopulation();
        int __popDifference = _population.Count - _initialPopulation;
        if (__popDifference > 0)
        {
            for (int i = 1; i < __popDifference + 1; i++)
            {
                _population.RemoveAt(_population.Count - i);
            }
        }
    }

    // Instruções:
    // Quando for codar as funções (de seleção) trate como se a população ja tenha sido criada
    // Ou seja, quando fores escrever o codigo, deves ter em mente que  na variavel _population 
    // Já existem 100 ("pessoas") com fitnes (grau de aptidões variadas)
    // Já existem metodos (funções) que destroem a população e que ordenam ela por ordem de aptidão (fitness)
    // Essas são: DestroyPopulation() e SortPopulation(). Ambas neste mesmo script
    // Para testar as funções que tu virá a escrever, tu terás que fazer uma alteração na linha 524 (524 no momento da escrita deste documento)
    // Lá esta escrito: _tProcreation = new Timer(_procreationStep * 2, true, Procreate);
    // Para testar as tuas funções subtitua a palavra Procreate, pelo nome da função de seleção que tu desejas testar.
    // Lembrando que como se trata de evento nao precisa colocar os parenteses '()' após o nome da função. Exemplo:
    // _tProcreation = new Timer(_procreationStep * 2, true, ProportionalSelection);
    // Sinta-se a vontade para criar qualquer variavel e função que sintas a necessidade.
    // Lembre-se de printar informações no console e acompanhar a atualização das variaveis através dele e da interface da tela de jogo.
    // Qualquer duvida ou pergunta, estamos eu e giovanni a disposiçãolesser

    /// <summary>
    /// Codar aqui o corpo do metodo (função) para seleção proporcional
    /// </summary>
    private Agent[] ProportionalSelection()
    {
        int[] __proportionalFitness = new int[_population.Count];
        int __lowerFitness = int.MaxValue;
        int __upperFitness = int.MinValue;

        for (int i = 0; i < _population.Count; i++)
        {
            if (_population[i].fitness >= __upperFitness) __upperFitness = _population[i].fitness;
            if (_population[i].fitness <= __lowerFitness) __lowerFitness = _population[i].fitness;

            __proportionalFitness[i] = _population[i].fitness;
        }

        long __fitnessTotal = 0;
        for (int i = 0; i < __proportionalFitness.Length; i++)
        {
            __proportionalFitness[i] += __lowerFitness;
            __fitnessTotal += __proportionalFitness[i];
        }

        int __firstNumber = (int)Random.Range(0, __fitnessTotal);
        int __secondNumber = (int)Random.Range(0, __fitnessTotal);

        int __dadIndex = 0;
        int __momIndex = 0;
        int __currentValue = 0;
        for (int i = 0; i < __proportionalFitness.Length; i++)
        {
            __currentValue += __proportionalFitness[i];
            if (__currentValue >= __firstNumber)
            {
                __dadIndex = i;
                break;
            }
        }

        __currentValue = 0;
        for (int i = 0; i < __proportionalFitness.Length; i++)
        {
            __currentValue += __proportionalFitness[i];
            if (__currentValue >= __secondNumber)
            {
                __momIndex = i;
                break;
            }
        }

        return (new Agent[2] { _population[__dadIndex], _population[__momIndex] });
    }

    /// <summary>
    /// Codar aqui o corpo do metodo (função) para seleção por torneio
    /// </summary>
    private Agent[] TournamentSelection()
    {
        List<Agent> __selectionPoll = _population;
        List<Agent> __tournament = new List<Agent>();

        int __firstPlace = int.MinValue;
        int __secondPlace = int.MinValue;
        int __dadIndex = 0, __momIndex = 0;

        IA_Project.Utility.Functions.Shuffle(__selectionPoll);
        for (int i = 0; i < _tournamentSize; i++)
        {
            __tournament.Add(__selectionPoll[i]);

            if(__tournament[i].fitness > __firstPlace)
            {
                if(__firstPlace != int.MinValue)
                {
                    __secondPlace = __firstPlace;
                }

                __firstPlace = __tournament[i].fitness;
                __dadIndex = i;
            }
            else if(__tournament[i].fitness > __secondPlace)
            {
                __secondPlace = __tournament[i].fitness;
                __momIndex = i;
            }
        }

        return (new Agent[2] { __tournament[__dadIndex], __tournament[__momIndex] });
    }

    /// <summary>
    /// Codar aqui o corpo do metodo (função) para seleção por truncamento
    /// </summary>
    private Agent[] TruncationSelection()
    {
        int __upperFitness = int.MinValue;

        for (int i = 0; i < _population.Count; i++)
        {
            if (_population[i].fitness > __upperFitness) __upperFitness = _population[i].fitness;
        }

        int __cutValue = Mathf.CeilToInt(__upperFitness * _truncationAmmount);

        List<Agent> __truncationPool = new List<Agent>();
        for (int i = 0; i < _population.Count; i++)
        {
            if (_population[i].fitness >= __cutValue)
                __truncationPool.Add(_population[i]);
        }

        IA_Project.Utility.Functions.Shuffle(__truncationPool);

        return (new Agent[2] { __truncationPool[0], __truncationPool[1] });
    }

    /// <summary>
    /// Codar aqui o corpo do metodo (função) para seleção por nivelamento linear
    /// </summary>
    private void LinearLevellingSelection()
    {

    }

    /// <summary>
    /// Receives two agents and returns the product of the two
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    private Agent[] CrossGenes(Agent a, Agent b, int split)
    {
        List<Agent> __product = new List<Agent>();
        __product.Add(new Agent(new List<List<TileData>>()));
        __product.Add(new Agent(new List<List<TileData>>()));

        for (int i = 0; i < a.Count; i++)
        {
            List<TileData> __column = new List<TileData>();

            if (i < split)
            {
                __product[0].Add(a[i]);
                __product[1].Add(b[i]);
            }
            else
            {
                __product[0].Add(b[i]);
                __product[1].Add(a[i]);
            }
        }

        int __mutationAmount = (int)((_generatorTempPlsChange.MapSize * _generatorTempPlsChange.MapSize) * _mutationRate);
        for (int i = 0; i < __mutationAmount; i++)
        {
            int __xIndex = Random.Range(0, _generatorTempPlsChange.MapSize);
            int __yIndex = Random.Range(0, _generatorTempPlsChange.MapSize);

            __product[0][__xIndex][__yIndex] = _generatorTempPlsChange.GetRandomTile();

            __xIndex = Random.Range(0, _generatorTempPlsChange.MapSize);
            __yIndex = Random.Range(0, _generatorTempPlsChange.MapSize);

            __product[1][__xIndex][__yIndex] = _generatorTempPlsChange.GetRandomTile();
        }

        for (int i = 0; i < __product.Count; i++)
        {
            Agent __a = __product[i];
            Evaluate(ref __a);
        }

        return (__product.ToArray());
    }

    //Evaluation

    private int ReEvaluateAdjacency(ref Agent agent)
    {
        int __adjFitness = 0;
        int __directionSize = System.Enum.GetNames(typeof(Directions)).Length;

        for (int i = 0; i < agent.Count; i++)
        {
            for (int j = 0; j < agent[i].Count; j++)
            {
                InterestMatrix __adjMatrix = new InterestMatrix(agent[i][j].type);
                __adjMatrix = InterestMatrix.RotateMatrix(__adjMatrix, (int)agent[i][j].direction);

                for (int direction = 0; direction < __directionSize; direction++)
                {
                    DirectionCoord __direction = new DirectionCoord((Directions)i);
                    if (__adjMatrix.ValidAtIndex(__direction))
                    {
                        if (i + __direction.X >= 0 && i + __direction.X < agent.Count)
                        {
                            if (j + __direction.Y >= 0 && j + __direction.Y < agent[i].Count)
                            {
                                __adjFitness += ScoreAdjacency(agent[i][j], agent[i + __direction.X][j + __direction.Y], __direction.Dir, true);
                            }
                        }
                    }
                }
            }
        }

        return (__adjFitness);
    }

    /// <summary>
    /// Aplies a score based on the block next to the root tile. Quick mode as the name implies is quicker but has less options for scoring
    /// </summary>
    /// <param name="root"></param>
    /// <param name="comp"></param>
    /// <param name="rootToComp"></param>
    /// <param name="quick"></param>
    /// <returns></returns>
    private int ScoreAdjacency(TileData root, TileData comp, Directions rootToComp, bool quick = false)
    {
        int __localScore = 0;
        if (quick)
        {
            InterestMatrix __rootMatrix = new InterestMatrix(root.type);
            __rootMatrix = InterestMatrix.RotateMatrix(__rootMatrix, (int)root.direction);

            InterestMatrix __compMatrix = new InterestMatrix(comp.type);
            __compMatrix = InterestMatrix.RotateMatrix(__compMatrix, (int)comp.direction);

            switch (rootToComp)
            {
                case Directions.Up:
                    {
                        if (__rootMatrix.ValidAtIndex(0, 1) &&
                            __compMatrix.ValidAtIndex(2, 1))
                        {
                            __localScore += _defaultQuickScore;
                        }
                        else
                        {
                            __localScore -= _defaultQuickScore;
                        }
                    }
                    break;

                case Directions.Right:
                    {
                        if (__rootMatrix.ValidAtIndex(1, 2) &&
                            __compMatrix.ValidAtIndex(1, 0))
                        {
                            __localScore += _defaultQuickScore;
                        }
                        else
                        {
                            __localScore -= _defaultQuickScore;
                        }
                    }
                    break;

                case Directions.Down:
                    {
                        if (__rootMatrix.ValidAtIndex(2, 1) &&
                            __compMatrix.ValidAtIndex(0, 1))
                        {
                            __localScore += _defaultQuickScore;
                        }
                        else
                        {
                            __localScore -= _defaultQuickScore;
                        }
                    }
                    break;

                case Directions.Left:
                    {
                        if (__rootMatrix.ValidAtIndex(1, 0) &&
                            __compMatrix.ValidAtIndex(1, 2))
                        {
                            __localScore += _defaultQuickScore;
                        }
                        else
                        {
                            __localScore -= _defaultQuickScore;
                        }
                    }
                    break;
            }
        }
        else
        {

        }
        return (__localScore);
    }

    #endregion

    #region Public Methods

    public void PauseAlgorithm()
    {
        _tProcreation.Pause();
    }
    public void ResumeAlgorithm()
    {
        _tProcreation.Play();
    }
    public void RunAlgorithm()
    {
        CreatePopulation(_initialPopulation);

        if (_crossoverMethod == CrossoverMethod.One_point) _crossoverCallback = OnePointCrossover;
        else _crossoverCallback = TwoPointCrossover;

        switch(_selectionMethod)
        {
            case SelectionMethod.Proportional:
                {
                    _selectionCallback = ProportionalSelection;
                }
                break;

            case SelectionMethod.Tournament:
                {
                    _selectionCallback = TournamentSelection;
                }
                break;

            case SelectionMethod.Truncation:
                {
                    _selectionCallback = TruncationSelection;
                }
                break;
        }

        _procreationIndex = new SelectionPair(0, _population.Count - 1);
        _tProcreation = new Timer(_procreationStep * 2, true, Procreate);

        ModuleManager.Play(_tProcreation);
    }

    #endregion

    #region Private Methods

    //Utility

    /// <summary>
    /// Sorts the current population list in order from greater to lesser fitness
    /// </summary>
    private void SortPopulation()
    {
        _population.Sort();
        _population.Reverse();
    }
    private void DestroyPopulation()
    {

    }
    private float CalculateAvgFitness()
    {
        float __sum = 0;
        for (int i = 0; i < _population.Count; i++)
        {
            __sum += _population[i].fitness / _population.Count;
        }
        
        return (__sum);
    }


    //Deprecated
    private IEnumerator ScreenCap(float wait)
    {
        yield return new WaitForSeconds(wait);
        SaveAgentImage();
    }
    private void SaveAgentImage()
    {
        var __upperDir = System.IO.Directory.GetParent(Application.dataPath);
        var __path = __upperDir + "/Generations/_Agent_" + _agntIndex + ".png";

        ScreenCapture.CaptureScreenshot(__path);
        _agntIndex++;
    }
    #endregion
}