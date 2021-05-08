using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using PathCreation;



public class FooEnv : MonoBehaviour
{
    public GameObject agent;

    [Header("Random Anchors & BezierPath Parameters")]
    public int num_anchors;                 // default 10
    public float radius_anchor_circle;      // default 8.0
    public float radius_epsilon_ratio;      // default 0.7
    public float theta_epsilon_ratio;       // default 0.7
    public float max_anchor_height;         // default 1.0, only for 3d path
    public float max_anchor_angle;          // default 15.0, only for 3d path
    public PathSpace path_space;            // default PathSpace.xz
    public float road_width;                // default 1.0
    [Header("Agent Setting")]
    public float agent_mass;                // default 1.0
    public float force_multiplier;          // default 10
    public int ticker_start;                // default -3
    public int ticker_end;                  // default 5
    public float ticker_space;              // default 0.2
    public float running_penalty;           // default -5.0
    public float failure_penalty;           // default -100.0
    [Header("Friction Accident Event Parameters")]
    public bool enableFrictionAccident;     // default false
    public float frictionchanged_time;      // default 3.0
    public float static_friction;           // default 0.0
    public float dynamic_friction;          // default 0.0
    [Header("Wind Accident Event Parameters")]
    public bool enableWindAccident;         // default false
    public float wind_time;                 // default 3.0
    public float wind_force_X;              // default 0.0
    public float wind_force_Y;              // default 0.0, only for 3d path
    public float wind_force_Z;              // default 0.0
    [Header("Loss Control Accident Event Parameter")]
    public bool enableLossControlAccident;  // default false
    public float lossctrl_time;             // default 3.0
    public float lossctrl_Xaxis_ratio;      // default 1.0
    public float lossctrl_Zaxis_ratio;      // default 1.0

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
        road = new RoadObject(anchors, angles, true, path_space, BezierPath.ControlMode.Automatic, road_width);
        road.parentObject = gameObject;
        SetRoadPhysicalMaterial(1.0f, 1.0f);
        SetAgentOnRandomAnchor();
        InitializeCarAgentComponent();
        agent.gameObject.SetActive(true);
    }

    List<Vector3> GetRandomAnchors(float _radius, int _num_anchors)
    {
        List<Vector3> anchors = new List<Vector3>();

        float radius = _radius;
        float r_epsilon = radius * radius_epsilon_ratio;
        float delta_theta = Mathf.PI * 2 / _num_anchors;
        float t_epsilon = delta_theta * theta_epsilon_ratio;

        for (int i = 0; i < _num_anchors; i++)
        {
            float radius_i = Random.Range(radius - r_epsilon, radius + r_epsilon);
            float theta_i = Random.Range(i * delta_theta - t_epsilon, i * delta_theta + t_epsilon);
            float x = radius_i * Mathf.Cos(theta_i);
            float y = (path_space == PathSpace.xyz) ? Random.Range(0, max_anchor_height) : 0.0f;
            float z = radius_i * Mathf.Sin(theta_i);

            anchors.Add(new Vector3(x, y, z));
        }

        return anchors;
    }
    List<float> GetRandomAngles(int _num_anchors)
    {
        List<float> angles = new List<float>();
        for (int i = 0; i < _num_anchors; i++)
            angles.Add(Random.Range(-max_anchor_angle, +max_anchor_angle));

        return angles;
    }
    void SetAgentOnRandomAnchor()
    {
        List<Vector3> anchors = this.road.GetAnchors();
        Vector3 position = anchors[Random.Range(0, anchors.Count)];
        FooAgent fooAgent = agent.GetComponent<FooAgent>();
        
        switch(fooAgent.agentType)
        {
            case FooAgent.AgentType.Ball:
                position.y += 0.5f;
                break;
            case FooAgent.AgentType.Vehicle:

                position.y += 1.0f;
                break;
            default:
                break;
        }
        
        agent.transform.localPosition = position;
    }
    void InitializeCarAgentComponent()
    {
        FooAgent component = agent.GetComponent<FooAgent>();
        component.vertexPath = this.road.pathCreator.path;
        component.startPosition = agent.transform.localPosition;
    }
    void SetRoadPhysicalMaterial(float sf, float df)
    {
        PhysicMaterial material = new PhysicMaterial();
        material.staticFriction = sf;
        material.dynamicFriction = df;
        material.frictionCombine = PhysicMaterialCombine.Average;

        road.physicalMaterial = material;
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
        ClearAgentAccident();
        RegisterAccidents();
    }

    // Accident events
    void RegisterAccidents()
    {
        Invoke("FrictionAccident", frictionchanged_time);
        Invoke("WindAccident", wind_time);
        Invoke("LossControlAccident", lossctrl_time);
    }
    void FrictionAccident()
    {
        if (!enableFrictionAccident) return;

        Debug.Log("FrictionAccident happen !");

        SetRoadPhysicalMaterial(static_friction, dynamic_friction);
    }
    void WindAccident()
    {
        if (!enableWindAccident) return;

        Debug.Log("WindAccident happen !");
        FooAgent component = agent.GetComponent<FooAgent>();
        Vector3 windForce = new Vector3();
        windForce.x = wind_force_X;
        windForce.y = (path_space == PathSpace.xz) ? 0.0f : wind_force_Y;
        windForce.z = wind_force_Z;
        component.windForce = windForce;
    }
    void LossControlAccident()
    {
        if (!enableLossControlAccident) return;

        Debug.Log("LossControl happen !");
        FooAgent component = agent.GetComponent<FooAgent>();
        component.ctrlXaxisMultiplier = lossctrl_Xaxis_ratio;
        component.ctrlZaxisMultiplier = lossctrl_Zaxis_ratio;
    }
    // Configuration
    void SetupCarEnvConfiguration()
    {
        EnvironmentParameters parameters = Academy.Instance.EnvironmentParameters;
        // Random Anchors & BezierPath Parameters
        num_anchors = (int)parameters.GetWithDefault("num_anchors", 10.0f);
        radius_anchor_circle = parameters.GetWithDefault("radius_anchor_circle", 20.0f);
        radius_epsilon_ratio = parameters.GetWithDefault("radius_epsilon_ratio", 0.7f);
        theta_epsilon_ratio = parameters.GetWithDefault("theta_epsilon_ratio", 0.7f);
        max_anchor_height = parameters.GetWithDefault("max_anchor_height", 3.0f);
        max_anchor_angle = parameters.GetWithDefault("max_anchor_angle", 15.0f);
        path_space = (PathSpace)parameters.GetWithDefault("path_space", (float)PathSpace.xz);
        road_width = parameters.GetWithDefault("road_width", 5.0f);
        // Agent Setting
        agent_mass = parameters.GetWithDefault("agent_mass", 1.0f);
        force_multiplier = parameters.GetWithDefault("force_multiplier", 10.0f);
        ticker_start = (int)parameters.GetWithDefault("ticker_start", -3.0f);
        ticker_end = (int)parameters.GetWithDefault("ticker_end", 5.0f);
        ticker_space = parameters.GetWithDefault("ticker_space", 0.0f);
        running_penalty = parameters.GetWithDefault("running_penalty", -5.0f);
        failure_penalty = parameters.GetWithDefault("failure_penalty", -100.0f);
        // Friction Accident Event Parameters
        enableFrictionAccident = (parameters.GetWithDefault("enableFrictionAccident", 0.0f) > 0.0f);
        frictionchanged_time = parameters.GetWithDefault("frictionchanged_time", 3.0f);
        static_friction = parameters.GetWithDefault("static_friction", 0.0f);
        dynamic_friction = parameters.GetWithDefault("dynamic_friction", 0.0f);
        // Wind Accident Event Parameters
        enableWindAccident = (parameters.GetWithDefault("enableWindAccident", 0.0f) > 0.0f);
        wind_time = parameters.GetWithDefault("wind_time", 3.0f);
        wind_force_X = parameters.GetWithDefault("wind_force_X", 0.0f);
        wind_force_Y = parameters.GetWithDefault("wind_force_Y", 0.0f);
        wind_force_Z = parameters.GetWithDefault("wind_force_Z", 0.0f);
        // Loss Control Accident Event Parameter
        enableLossControlAccident = (parameters.GetWithDefault("enableLossControlAccident", 0.0f) > 0.0f);
        lossctrl_time = parameters.GetWithDefault("lossctrl_time", 3.0f);
        lossctrl_Xaxis_ratio = parameters.GetWithDefault("lossctrl_Xaxis_ratio", 1.0f);
        lossctrl_Zaxis_ratio = parameters.GetWithDefault("lossctrl_Zaxis_ratio", 1.0f);
    }
    void SetupAgentConfiguration()
    {
        Rigidbody rigidbody = agent.GetComponent<Rigidbody>();
        rigidbody.mass = agent_mass;

        FooAgent carAgent = agent.GetComponent<FooAgent>();
        carAgent.xyz_mode = (path_space == PathSpace.xyz);
        carAgent.forceMultiplier = force_multiplier;
        carAgent.tickerStart = ticker_start;
        carAgent.tickerEnd = ticker_end;
        carAgent.running_penalty = running_penalty;
        carAgent.failure_penalty = failure_penalty;

        carAgent.windForce = Vector3.zero;
        carAgent.ctrlXaxisMultiplier = 1.0f;
        carAgent.ctrlZaxisMultiplier = 1.0f;
    }
    void ClearAgentAccident()
    {
        FooAgent carAgent = agent.GetComponent<FooAgent>();
        carAgent.windForce = Vector3.zero;
        carAgent.ctrlXaxisMultiplier = 1.0f;
        carAgent.ctrlZaxisMultiplier = 1.0f;
    }
}
