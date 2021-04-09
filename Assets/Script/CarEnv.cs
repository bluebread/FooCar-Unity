using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using PathCreation;



public class CarEnv : MonoBehaviour
{
    public GameObject agent;

    [Header("Random Anchors & BezierPath Parameters")]
    public int num_anchors;                 // default 10
    public float radius_anchor_circle;      // default 8.0
    public float radius_epsilon_multiplier; // default 0.7
    public float theta_epsilon_multiplier;  // default 0.7
    public float abs_anchor_height;         // default 1.0, only for 3d path
    public float abs_anchor_angle;          // default 15.0, only for 3d path
    public PathSpace pathSpace;             // default PathSpace.xz
    public float meshRoadWidth;             // default 1.0
    [Header("Agent Setting")]
    public float forceMultiplier;           // default 10
    public int tickerStart;                 // default -3
    public int tickerEnd;                   // default 5
    public float tickerSpace;               // default 0.2
    public float runningPenalty;            // default -5.0
    public float failurePenalty;            // default -100.0
    [Header("Friction Accident Event Parameters")]
    public bool enableFrictionAccident;     // default false
    public float FrictionAccidentTime;      // default 3.0
    public float staticFriction;            // default 0.0
    public float dynamicFriction;           // default 0.0
    [Header("Wind Accident Event Parameters")]
    public bool enableWindAccident;         // default false
    public float WindAccidentTime;          // default 3.0
    public float windForceXaxis;            // default 0.0
    public float windForceYaxis;            // default 0.0, only for 3d path
    public float windForceZaxis;            // default 0.0
    [Header("Loss Control Accident Event Parameter")]
    public bool enableLossControlAccident;  // default false
    public float LossControlTime;           // default 3.0
    public float ctrlXaxisMultiplier;       // default 1.0
    public float ctrlZaxisMultiplier;       // default 1.0

    // private
    RoadObject road;

    // Start is called before the first frame update
    void Start()
    {
        SetupCarEnvConfiguration();
        SetupAgentConfiguration();
        InitializeEnviroment();
        RegisterAccidents();
    }
    void InitializeEnviroment()
    {
        List<Vector3> anchors = GetRandomAnchors(radius_anchor_circle, num_anchors);
        List<float> angles = GetRandomAngles(num_anchors);
        road = new RoadObject(anchors, angles, true, pathSpace, BezierPath.ControlMode.Automatic, meshRoadWidth);
        road.parentObject = gameObject;
        SetAgentOnRandomAnchor();
        InitializeCarAgentComponent();
    }

    List<Vector3> GetRandomAnchors(float _radius, int _num_anchors)
    {
        List<Vector3> anchors = new List<Vector3>();

        float radius = _radius;
        float r_epsilon = radius * radius_epsilon_multiplier;
        float delta_theta = Mathf.PI * 2 / _num_anchors;
        float t_epsilon = delta_theta * theta_epsilon_multiplier;

        for (int i = 0; i < _num_anchors; i++)
        {
            float radius_i = Random.Range(radius - r_epsilon, radius + r_epsilon);
            float theta_i = Random.Range(i * delta_theta - t_epsilon, i * delta_theta + t_epsilon);
            float x = radius_i * Mathf.Cos(theta_i);
            float y = (pathSpace == PathSpace.xyz) ? Random.Range(0, abs_anchor_height) : 0.0f;
            float z = radius_i * Mathf.Sin(theta_i);

            anchors.Add(new Vector3(x, y, z));
        }

        return anchors;
    }
    List<float> GetRandomAngles(int _num_anchors)
    {
        List<float> angles = new List<float>();
        for(int i = 0; i < _num_anchors; i++)
            angles.Add(Random.Range(-abs_anchor_angle, +abs_anchor_angle));

        return angles;
    }
    void SetAgentOnRandomAnchor()
    {
        List<Vector3> anchors = this.road.GetAnchors();
        Vector3 position = anchors[Random.Range(0, anchors.Count)];
        position.y += 0.5f;
        
        agent.transform.localPosition = position;
    }
    void InitializeCarAgentComponent()
    {
        CarAgent component = agent.GetComponent<CarAgent>();
        component.vertexPath = this.road.pathCreator.path;
        component.startPosition = agent.transform.localPosition;
    }
    // Reset Enviroment
    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += ResetEnviroment;
    }
    void ResetEnviroment()
    {
        if (road != null && !road.Equals(default(RoadObject)))
            road.DestroyGameObject();
        InitializeEnviroment();
        RegisterAccidents();
    }

    // Accident events
    void RegisterAccidents()
    {
        Invoke("FrictionAccident", FrictionAccidentTime);
        Invoke("WindAccident", WindAccidentTime);
        Invoke("LossControlAccident", LossControlTime);
    }
    void FrictionAccident()
    {
        if (!enableFrictionAccident) return;

        Debug.Log("FrictionAccident happen !");
        PhysicMaterial material = new PhysicMaterial();
        material.staticFriction = staticFriction;
        material.dynamicFriction = dynamicFriction;
        material.frictionCombine = PhysicMaterialCombine.Average;

        road.physicalMaterial = material;
    }
    void WindAccident()
    {
        if (!enableWindAccident) return;

        Debug.Log("WindAccident happen !");
        CarAgent component = agent.GetComponent<CarAgent>();
        Vector3 windForce = new Vector3();
        windForce.x = windForceXaxis;
        windForce.y = (pathSpace == PathSpace.xz) ? 0.0f : windForceYaxis;
        windForce.z = windForceZaxis;
        component.windForce = windForce;
    }
    void LossControlAccident()
    {
        if (!enableLossControlAccident) return;

        Debug.Log("LossControl happen !");
        CarAgent component = agent.GetComponent<CarAgent>();
        component.ctrlXaxisMultiplier = ctrlXaxisMultiplier;
        component.ctrlZaxisMultiplier = ctrlZaxisMultiplier;
    }
    // Configuration
    void SetupCarEnvConfiguration()
    {
        EnvironmentParameters parameters = Academy.Instance.EnvironmentParameters;
        // Random Anchors & BezierPath Parameters
        num_anchors = (int)parameters.GetWithDefault("num_anchors", 10.0f);
        radius_anchor_circle = parameters.GetWithDefault("radius_anchor_circle", 8.0f);
        radius_epsilon_multiplier = parameters.GetWithDefault("radius_epsilon_multiplier", 0.7f);
        theta_epsilon_multiplier = parameters.GetWithDefault("theta_epsilon_multiplier", 0.7f);
        abs_anchor_height = parameters.GetWithDefault("abs_anchor_height", 3.0f);
        abs_anchor_angle = parameters.GetWithDefault("abs_anchor_angle", 15.0f);
        pathSpace = (PathSpace)parameters.GetWithDefault("pathSpace", (float)PathSpace.xz);
        meshRoadWidth = parameters.GetWithDefault("meshRoadWidth", 1.0f);
        // Friction Accident Event Parameters
        enableFrictionAccident = (parameters.GetWithDefault("enableFrictionAccident", 0.0f) > 0.0f);
        FrictionAccidentTime = parameters.GetWithDefault("FrictionAccidentTimeRange", 3.0f);
        staticFriction = parameters.GetWithDefault("staticFrictionRange", 0.0f);
        dynamicFriction = parameters.GetWithDefault("dynamicFrictionRange", 0.0f);
        // Wind Accident Event Parameters
        enableWindAccident = (parameters.GetWithDefault("enableWindAccident", 0.0f) > 0.0f);
        WindAccidentTime = parameters.GetWithDefault("WindAccidentTimeRange", 3.0f);
        windForceXaxis = parameters.GetWithDefault("windForceXaxisRange", 0.0f);
        windForceYaxis = parameters.GetWithDefault("windForceYaxisRange", 0.0f);
        windForceZaxis = parameters.GetWithDefault("windForceZaxisRange", 0.0f);
        // Loss Control Accident Event Parameter
        enableLossControlAccident = (parameters.GetWithDefault("enableLossControlAccident", 0.0f) > 0.0f);
        LossControlTime = parameters.GetWithDefault("LossControlTime", 3.0f);
        ctrlXaxisMultiplier = parameters.GetWithDefault("ctrlXaxisMultiplier", 1.0f);
        ctrlZaxisMultiplier = parameters.GetWithDefault("ctrlZaxisMultiplier", 1.0f);
        // Agent Setting
        forceMultiplier = parameters.GetWithDefault("forceMultiplier", 10.0f);
        tickerStart = (int)parameters.GetWithDefault("tickerStart", -3.0f);
        tickerEnd = (int)parameters.GetWithDefault("tickerEnd", 5.0f);
        tickerSpace = parameters.GetWithDefault("tickerSpace", 0.0f);
        runningPenalty = parameters.GetWithDefault("runningPenalty", -5.0f);
        failurePenalty = parameters.GetWithDefault("failurePenalty", -100.0f);
    }
    void SetupAgentConfiguration()
    {
        Debug.Log("SetupAgentConfiguration");

        CarAgent carAgent = agent.GetComponent<CarAgent>();
        carAgent.xyz_mode = (pathSpace == PathSpace.xyz);
        carAgent.forceMultiplier = forceMultiplier;
        carAgent.tickerStart = tickerStart;
        carAgent.tickerEnd = tickerEnd;
        carAgent.running_penalty = runningPenalty;
        carAgent.failure_penalty = failurePenalty;
    }
}
