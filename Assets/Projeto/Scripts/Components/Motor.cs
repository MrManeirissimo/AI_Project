using MindTrick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour {

    #region Constant Fields

    private float MinTorque = 0.1f;

    #endregion

    #region Serialized Fields

    [SerializeField] private MotorData _data;

    #endregion

    #region Private Fields

    [SerializeField,ReadOnly]private List<WheelCollider> _wheels = new List<WheelCollider>();
    [SerializeField,ReadOnly]private List<WheelCollider> _steerWheels = new List<WheelCollider>();

    private float _targetSteeringAngle;
    private float _targetAcceleration;

    private Rigidbody _rigidbody;

    #endregion

    #region Public Properties

    public MotorData Data
    {
        get
        {
            return _data;
        }
    }

    public float Velocity
    {
        get
        {
            return (_rigidbody.velocity.magnitude);
        }
    }

    #endregion

    #region Nested Delegates

    private delegate void WheelFunction(WheelCollider wheel);

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        WheelCollider[] __wheels = GetComponentsInChildren<WheelCollider>();

        for (int i = 0; i < __wheels.Length; i++)
        {
            _wheels.Add(__wheels[i]);
            switch (_data.SteerType)
            {
                case MotorData.SteerTypes.Front:
                    if (__wheels[i].gameObject.CompareTag(Generated.DictTags[TagType.WheelFront]))
                    {
                        _steerWheels.Add(__wheels[i]);
                    }
                    break;
                case MotorData.SteerTypes.Back:
                    if (__wheels[i].gameObject.CompareTag(Generated.DictTags[TagType.WheelBack]))
                    {
                        _steerWheels.Add(__wheels[i]);
                    }
                    break;
                default:
                    break;
            }
            
        }

        if(_wheels.Count == 0)
        {
            Debug.LogError("Motor has no wheels!");
        }

        Initialize();
    }

    private void FixedUpdate()
    {
        UpdateSteer();
        UpdateAcceleration();

    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        _targetSteeringAngle = 0;
    }

    public void Accelerate(float axis)
    {
        _targetAcceleration = _data.maxTorque* axis;
        if(Mathf.Abs(_targetAcceleration) <= MinTorque)
        {
            _targetAcceleration = 0;
        }
    }

    public void Turn(float axis)
    {
        _targetSteeringAngle = _data.maxTurnAngle * axis;
    }

    #endregion

    #region Private Methods

    private void UpdateAcceleration()
    {
        ForEachWheel(delegate (WheelCollider wheel)
        {
            wheel.motorTorque = _targetAcceleration;
        });
    }

    private void UpdateSteer()
    {
        float __currentAngle = _steerWheels[0].steerAngle;

        float __angle = Mathf.Lerp(__currentAngle, _targetSteeringAngle, Time.fixedDeltaTime * _data.TurnSpeed);

        ForEachSteerWheel(delegate (WheelCollider wheel)
        {
            wheel.steerAngle = __angle;
        });
    }

    private void ForEachWheel(WheelFunction callback)
    {
        if (callback != null)
        {
            for (int i = 0; i < _wheels.Count; i++)
            {
                callback.Invoke(_wheels[i]);
            }
        }
    }

    private void ForEachSteerWheel(WheelFunction callback)
    {
        if(callback != null)
        {
            for (int i = 0; i < _steerWheels.Count; i++)
            {
                callback.Invoke(_steerWheels[i]);
            }
        }
    }

    #endregion
}
