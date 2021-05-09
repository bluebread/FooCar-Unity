using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using PathCreation;

public class FooAgent : Agent
{
    public VertexPath vertexPath;
    public Vector3 startPosition;
    public Vector3 startAngles;

    // common parameters
    public bool xyz_mode = false;
    public float forceMultiplier = 10;
    public int tickerStart = -3;
    public int tickerEnd = 5;
    public float tickerSpace = 0.2f;
    public float running_penalty = -5.0f;
    public float failure_penalty = -100.0f;
    // ball accident parameters
    [Range(0, 1.0f)]
    public float ctrlXaxisMultiplier = 1.0f;
    [Range(0, 1.0f)]
    public float ctrlZaxisMultiplier = 1.0f;
    public Vector3 windForce = Vector3.zero;
    // vehicle accident parameters
    public bool LossLeftWheelBreak = false;
    public bool LossRightWheelBreak = false;
    // vehicle control input
    public GameObject vehicleControl;
    public float verticalInput
    {
        private get; set;
    }
    public float horizontalInput
    {
        private get; set;
    }
    public float spaceInput
    {
        private get; set;
    }

    // readonly variable
    public int ObservationSize
    {
        get
        {
            int basic_num = 6;
            int point_dim = xyz_mode ? 3 : 2;
            return basic_num + 2 * point_dim * (tickerEnd - tickerStart + 1);
        }
    }

    public enum AgentType { Ball, Vehicle };
    public AgentType agentType = AgentType.Vehicle;

    void Start()
    {
        //EnvironmentParameters parameters = Academy.Instance.EnvironmentParameters;
        ////agentType = (AgentType)(parameters.GetWithDefault("SceneName", 0.0f));
        //agentType = AgentType.Vehicle;
        //string SceneName = (agentType == AgentType.Ball) ? "BallScene" : "VehicleScene";
        //SceneManager.LoadScene(SceneName, LoadSceneMode.Single); // default "BallScene"
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Vector3 position2D = new Vector3(transform.position.x, 0, transform.position.z);
        float distance = vertexPath.GetClosestDistanceAlongPath(position2D);
        Vector3 direction = vertexPath.GetDirectionAtDistance(distance);
        float angle = Vector3.Angle(new Vector3(0, 0, 1), direction);
        transform.eulerAngles = new Vector3(0, angle, 0);
    }

    public override void OnEpisodeBegin()
    {
        //if (startPosition.Equals(default(Vector3)))
        //    startPosition = this.transform.localPosition;

        Rigidbody rBody = GetComponent<Rigidbody>();
        if (this.transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = startPosition;
            transform.eulerAngles = startAngles;
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // # Basic information
        Rigidbody rBody = GetComponent<Rigidbody>();
        // Agent velocity
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rBody.velocity);
        // # Collect Path's basic information
        float centerDistance = vertexPath.GetClosestDistanceAlongPath(this.transform.localPosition);
        Vector3 direction = vertexPath.GetDirectionAtDistance(centerDistance);
        // Check clockwise direction & determine ticker's start and end
        bool clockwise = (Vector3.Dot(rBody.velocity, direction) >= 0);
        int t_s = clockwise ? tickerStart : -tickerEnd;
        int t_e = clockwise ? tickerEnd : -tickerStart;
        // # Collect Path's observations
        for(int d = t_s; d <= t_e; d++)
        {
            float distance = centerDistance + d * tickerSpace;
            Vector3 point = vertexPath.GetPointAtDistance(distance, EndOfPathInstruction.Loop);
            Vector3 normal = vertexPath.GetNormalAtDistance(distance, EndOfPathInstruction.Loop);
            AddObservationInXYZMode(sensor, point - this.transform.localPosition);
            AddObservationInXYZMode(sensor, normal);
        }
        // # Padding observations
        // If custom setting will have too more observations and cause the buffer overflow, We
        // must warn user (in Pyhon interface level).
        // Here we are padding the observation buffer with all zero
        int MaxObservationSize = GetComponent<BehaviorParameters>().BrainParameters.VectorObservationSize;
        for (int i = 0; i < MaxObservationSize - ObservationSize; i++)
            sensor.AddObservation(0);
    }
    void AddObservationInXYZMode(VectorSensor sensor, Vector3 vec)
    {
        if (xyz_mode) // 3D
            sensor.AddObservation(vec);
        else // 2D
        {
            sensor.AddObservation(vec.x);
            sensor.AddObservation(vec.z);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        switch(agentType)
        {
            case AgentType.Ball: BallActionReceived(actionBuffers); break;
            case AgentType.Vehicle: VehicleActionReceived(actionBuffers); break;
            default: break;
        }
        Rigidbody rBody = GetComponent<Rigidbody>();
        if (this.transform.localPosition.y < 0)
        {
            SetReward(failure_penalty);
            EndEpisode();
        }
        else
        {
            SetReward(rBody.velocity.sqrMagnitude + running_penalty);
        }
    }
    public void BallActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Rigidbody rBody = GetComponent<Rigidbody>();
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0] * ctrlXaxisMultiplier;
        controlSignal.z = actionBuffers.ContinuousActions[1] * ctrlZaxisMultiplier;
        rBody.AddForce(controlSignal * forceMultiplier + windForce);
    }
    
    public void VehicleActionReceived(ActionBuffers actionBuffers)
    {
        if (!vehicleControl) return;

        Rigidbody rBody = GetComponent<Rigidbody>();
        MSSceneControllerFree sceneController = vehicleControl.GetComponent<MSSceneControllerFree>();
        sceneController.verticalInput = actionBuffers.ContinuousActions[0];
        sceneController.horizontalInput = actionBuffers.ContinuousActions[1];
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
