using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using PathCreation;


[ExecuteInEditMode]
public class Tester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDisable()
    {
        //Debug.Log("PrintOnDisable: script was disabled");
    }
    public GameObject target;
    public Vector3 direction = new Vector3(1, 0, 1);
    void OnEnable()
    {
        Debug.Log("PrintOnEnable: script was enabled");
        float angle = Vector3.Angle(new Vector3(0, 0, 1), direction);
        target.transform.eulerAngles = new Vector3(0, angle, 0);
    }
    
}
