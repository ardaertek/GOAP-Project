using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Header("Stats Data")]
    [SerializeField] private EnemyDataSO _datas;


    [Header("Sensors")]
    [SerializeField] private Sensor _chaseSensor;
    [SerializeField] private Sensor _attackSensor;


    [Header("Know Locations")]
    [SerializeField] Transform _restingPosition;
    [SerializeField] Transform _foodShackPosition;

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private GameObject _target;
    private Vector3 _destination;
    private IGoapPlanner _goapPlanner;

    //serializefield cause debugging on Inspector about current Goal
    [ReadOnly] private AgentGoal _currentGoal;
    [ReadOnly] private AgentAction _currentAction;
    [ReadOnly] private ActionPlanning _actionPlan;
    private AgentGoal _lastGoal;
    private CountdownTimer _countdownTimer;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> Actions;
    public HashSet<AgentGoal> Goals;

    public float Health { get => _datas.CurrentHealth;}

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
       _navMeshAgent = GetComponent<NavMeshAgent>();
        _goapPlanner = new GoapPlanner();
    }

    private void Start()
    {
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    private void OnEnable()
    {
        _chaseSensor.OnTargetChanged += HandleTargetChanged;
    }
    private void OnDisable()
    {
        _chaseSensor.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleTargetChanged()
    {
        Debug.Log("Target Changed ,clearing action and goals");

        _currentAction = null;
        _currentGoal = null;
    }


    private void SetupTimers()
    {
        _countdownTimer = new CountdownTimer(2f);
        _countdownTimer.OnTimerStop += () =>
        {
            UpdateStats();
            _countdownTimer.Start();
        };
        _countdownTimer.Start();
    }

    private void UpdateStats()
    {
        if(InRangeOf(_restingPosition.position, 3f))
        {
            _datas.StaminaHandler(30);
        }
        else
        {
            _datas.StaminaHandler(-10);
        }

        if (InRangeOf(_foodShackPosition.position, 3f))
        {
            _datas.HealthHandler(20);
        }
        else
        {
            _datas.HealthHandler(-5);
        }
    }

    private bool InRangeOf(Vector3 pos,float range) => Vector3.Distance(transform.position,pos) < range;
    private void SetupBeliefs()
    {
        beliefs = new Dictionary<string, AgentBelief>();

        BeliefFactory factory = new BeliefFactory(this, beliefs);
        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("Agent Idle", () => !_navMeshAgent.hasPath);
        factory.AddBelief("Agent Moving", () => _navMeshAgent.hasPath);
        factory.AddBelief("Agent Health Low", () => _datas.CurrentHealth < 30);
        factory.AddBelief("Agent Is Healty", () => _datas.CurrentHealth >= 50);
        factory.AddBelief("Agent Stamina Low", () => _datas.CurrentStamina < 10);
        factory.AddBelief("Agent Is Rested", () => _datas.CurrentStamina >= 50);

        factory.AddLocationBelief("Agent At Rest Room",2f,_restingPosition);
        factory.AddLocationBelief("Agent At Food Shack", 2f, _foodShackPosition);

        factory.AddSensorBelief("Player In Chase Range", _chaseSensor);
        factory.AddSensorBelief("Player In Attack Range", _attackSensor);

        factory.AddBelief("Agent Attacking To Player", () => false);

    }
    private void SetupActions()
    {
        Actions = new HashSet<AgentAction>
        {
            new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(2))
            .AddEffect(beliefs["Nothing"])
            .Build(),

            new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(_navMeshAgent, 10))
            .AddEffect(beliefs["Agent Moving"])
            .Build(),

            new AgentAction.Builder("Move Eat Position")
            .WithStrategy(new MoveStrategy(_navMeshAgent, () => _foodShackPosition.position))
            .AddEffect(beliefs["Agent At Food Shack"])
            .Build(),

            new AgentAction.Builder("Eating")
            .WithStrategy(new EatStrategy(this))
            .AddPrecondition(beliefs["Agent At Food Shack"])
            .AddEffect(beliefs["Agent Is Healty"])
            .Build()
        };
    }
    private void SetupGoals()
    {
        Goals = new HashSet<AgentGoal>
        {
            new AgentGoal.Builder("Chill Out")
            .WithPriorty(1)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build(),
            new AgentGoal.Builder("Wander")
            .WithPriorty(1)
            .WithDesiredEffect(beliefs["Agent Moving"])
            .Build(),
            new AgentGoal.Builder("Keep Healt Up")
            .WithPriorty(2)
            .WithDesiredEffect(beliefs["Agent Is Healty"])
            .Build()
        };
    }


    private void Update()
    {
        _countdownTimer.Tick(Time.deltaTime);

        if(_currentAction == null)
        {
            Debug.Log("Calculating Plan");
            CalculatePlan();

            if(_actionPlan != null && _actionPlan.Actions.Count > 0)
            {
                _navMeshAgent.ResetPath();

                _currentGoal = _actionPlan.AgentGoal;
                Debug.Log("Goal : " + _currentGoal.Name + " Total Action In Plan :" + _actionPlan.Actions.Count);
                int i = 1;
                foreach(var goal in _actionPlan.Actions)
                {
                    Debug.Log("Goal " + i + " => " + goal.Name);
                    i++;
                }
                _currentAction = _actionPlan.Actions.Pop();
                Debug.Log("Popped action : " + _currentAction.Name);
                _currentAction.Start();
            }
        }

        //if we have action execute it
        if(_actionPlan != null && _currentAction != null)
        {
            _currentAction.Update(Time.deltaTime);

            if (_currentAction.Complate)
            {
                Debug.Log(_currentAction.Name + " is Complated");
                _currentAction.Stop();
                _currentAction = null;

                if (_actionPlan.Actions.Count == 0)
                {
                    Debug.Log("Plan Complated");
                    _lastGoal = _currentGoal;
                    _currentGoal = null;
                }
            }
        }
    }

    private void CalculatePlan()
    {
        var priortyLevel = _currentGoal?.Priorty ?? 0;

        HashSet<AgentGoal> goalsToCheck = Goals;

        //if has a plan just checking for priortty

        if(_currentGoal != null)
        {
            Debug.Log("Current Goal Already Exist , Checking for higher priorty");
            goalsToCheck = new HashSet<AgentGoal>(Goals.Where(g => g.Priorty > priortyLevel));
        }
        var plan = _goapPlanner.Plan(this,goalsToCheck,_lastGoal);

        if(plan != null)
        {
            _actionPlan = plan;
        }
    }
}
