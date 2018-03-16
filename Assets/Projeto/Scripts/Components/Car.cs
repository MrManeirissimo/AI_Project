using Fuzzy;
using MindTrick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{

    #region Serialized Fields

    //[SerializeField] private CarData _data;

    [SerializeField] private Transform _target;

    [SerializeField] private List<Transform> _listCurves = new List<Transform>();

    [SerializeField] private FuzzyBehaviour _fuzzy;

    #endregion

    #region Private Fields

    private Motor _motor;

    private int _currentPointIndex;

    private Timer _timer;

    private Concept _velocity;
    private Concept _distance;
    private Concept _breakDesire;

    public float AccForce = 1;

    #endregion

    #region Nested Classes

    private class CarState
    {

        #region Public Methods

        protected virtual void Enter()
        {

        }

        protected virtual void Exit()
        {

        }

        #endregion

    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        _motor = GetComponent<Motor>();

        _velocity = _fuzzy.GetConcept("Velocidade");

        _distance = _fuzzy.GetConcept("Distancia");

        _breakDesire = _fuzzy.GetConcept("BreakDesire");
    }

    private void Start()
    {
        Initialize(CarMapManager.Instance.GetCurveArray);
    }

    private void Update()
    {
        if (_target != null)
        {
            Vector3 __target = _target.position;
            __target.y = 0;

            Vector3 __position = transform.position;
            __position.y = 0;

            float __distance = Vector3.Distance(__target, __position);

            _velocity.Value = GetComponent<Rigidbody>().velocity.magnitude * 10;
            _distance.Value = __distance;

            UpdateTurn();

            UpdateTargetTile();

            //_fuzzy.ExecuteConditions();
            _motor.Accelerate(AccForce);
        }
    }

    #endregion

    #region Private Methods
    
    private void UpdateTurn()
    {
        Vector3 __reference = _target.transform.position - transform.position;
        __reference.y = 0;
        __reference.Normalize();

        Vector3 __origin = transform.forward;
        __origin.y = 0;
        __origin.Normalize();

        bool __left = transform.InverseTransformPoint(_target.transform.position).x < 0 ? true : false;
        float __angle = Vector3.Angle(__reference, __origin);


        float __intensity = Mathf.Clamp(__angle/ _motor.Data.maxTurnAngle, 0, 1);

        _motor.Turn(__left ? __intensity * -1 : __intensity);
    }

    private void UpdateTargetTile()
    {
        if (_distance.Value < 2.5f)
        {
            _currentPointIndex++;

            if(_listCurves.Count > _currentPointIndex)
            {
                _target = _listCurves[_currentPointIndex];
            }
            else
            {
                HandlePathOver();
            }
        }
    }

    private void HandlePathOver()
    {
        _target = null;

        _motor.Accelerate(0);
        Debug.Log("Path over");
    }

    private void GetBreakDesire()
    {
        _breakDesire.GetFunction("Break").Fuzzy = (Logic.OR(Logic.OR(Logic.AND(_velocity.IS("Alta"), _distance.IS("Perto")), Logic.AND(_velocity.IS("Media"), _distance.IS("Perto"))), _distance.IS("Muito Perto")));

        _breakDesire.GetFunction("Dont").Fuzzy = (Logic.OR(_velocity.IS("Baixa"), _distance.IS("Longe")));

        float __defuzzy = _breakDesire.Defuzzyfication(0.01f);
        Debug.Log("Break: " + _breakDesire.GetFunction("Break").Fuzzy + " | Dont: " + _breakDesire.GetFunction("Dont").Fuzzy + " | Defuzzy: " + __defuzzy);

        if(_breakDesire.GetFunction("Break").Fuzzy >= 0.85f)
        {
            AccForce = -1 * (_breakDesire.GetFunction("Break").Fuzzy) + 0.12f;
        }
        else if(_breakDesire.GetFunction("Break").Fuzzy < 0.85f && _breakDesire.GetFunction("Dont").Fuzzy >= 0.2f)
        {
            AccForce = 1 * _breakDesire.GetFunction("Dont").Fuzzy + 0.25f;
        }
        else
        {
            AccForce = 1;
        }
        //float __percentage;
        //if (__defuzzy <= 0.2f)
        //{
        //    __percentage = 1 * (0.2f / __defuzzy);
        //    AccForce = -__percentage;
        //}
        //else
        //{
        //    __percentage = 1 * __defuzzy / 0.8f;
        //    AccForce = __percentage;
        //}
    }

    #endregion

    #region Public Methods

    public void Initialize(Transform[] path)
    {
        _listCurves = new List<Transform>(path);

        _timer = new Timer(1f, true, delegate
        {
            GetBreakDesire();
        });
        _timer.Play();

        _currentPointIndex = 0;

        if(_listCurves.Count > 0)
        {
            _target = _listCurves[_currentPointIndex];
        }
    }

    #endregion
}
