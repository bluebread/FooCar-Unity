using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using PathCreation;


[ExecuteInEditMode]
public class Tester : MonoBehaviour
{
    int count = 0;
    int max_count = 5000;
    // Start is called before the first frame update
    RoadObject road;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (count % 200 == 0)
            Debug.Log("Tester update counter: " + count);
        if (count <= 0)
        {
            if (road != null && !road.Equals(default(RoadObject)))
                road.DestroyGameObject();
            //road = new RoadObject(GetRandomAnchors(8.0f, 10), true, PathSpace.xz, BezierPath.ControlMode.Automatic, 1.0f);
        }

        count = (count + 1) % max_count;

    }
    void OnDisable()
    {
        //Debug.Log("PrintOnDisable: script was disabled");
    }

    void OnEnable()
    {
        //Debug.Log("PrintOnEnable: script was enabled");
    }

    List<Vector3> GetRandomAnchors(float _radius, int _num_anchors)
    {
        List<Vector3> anchors = new List<Vector3>();

        float radius = _radius;
        float r_epsilon = radius * 0.7f;
        float delta_theta = Mathf.PI * 2 / _num_anchors;
        float t_epsilon = delta_theta * 0.7f;

        for (int i = 0; i < _num_anchors; i++)
        {
            float radius_i = Random.Range(radius - r_epsilon, radius + r_epsilon);
            float theta_i = Random.Range(i * delta_theta - t_epsilon, i * delta_theta + t_epsilon);
            float x = radius_i * Mathf.Cos(theta_i);
            float y = 0.0f;
            float z = radius_i * Mathf.Sin(theta_i);

            anchors.Add(new Vector3(x, y, z));
        }

        return anchors;
    }
}
