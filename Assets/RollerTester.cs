using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class RollerTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.mass = Academy.Instance.EnvironmentParameters.GetWithDefault("mass", 2.0f);
        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        collider.material = new PhysicMaterial();
        collider.material.staticFriction = 1.0f;
        collider.material.dynamicFriction = 1.0f;
        collider.material.frictionCombine = PhysicMaterialCombine.Average;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
