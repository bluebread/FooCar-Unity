using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class FloorTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
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
