using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorData : ScriptableObject {

    #region Nested Enum

    public enum SteerTypes
    {
        Front,
        Back
    }

    #endregion

    #region Serialized Fields

    [Header("Acceleration")]
    [SerializeField] private float _maxTorque = 30;
    [SerializeField] private float _acceleration = 30;
    [SerializeField] private float _breakForce = 60;

    [Header("Steering")]
    [SerializeField] private float _maxTurnAngle = 45;
    [SerializeField] private float _turnSpeed = 10;
    [SerializeField] private SteerTypes _steerType;

    #endregion

    #region Public Properties

    public float maxTorque
    {
        get
        {
            return (_maxTorque);
        }
    }

    public float maxTurnAngle
    {
        get
        {
            return _maxTurnAngle;
        }
    }

    public SteerTypes SteerType
    {
        get
        {
            return _steerType;
        }
    }

    public float TurnSpeed
    {
        get
        {
            return _turnSpeed;
        }
    }

    public float acceleration
    {
        get
        {
            return _acceleration;
        }
    }

    public float breakForce
    {
        get
        {
            return _breakForce;
        }
    }

    #endregion
}
