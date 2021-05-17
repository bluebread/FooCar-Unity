using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using PathCreation;


[ExecuteInEditMode]
public class Tester : MonoBehaviour
{
    public int BallNumber = 0;
    private string BallName
    {
        get
        {
            return "Ball(" + BallNumber + ")";
        }
    }
    private void Awake()
    {
        Debug.Log(BallName + ": Awake");

    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(BallName + ": Start");
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDisable()
    {
        //Debug.Log("PrintOnDisable: script was disabled");
    }
    void OnEnable()
    {
        Debug.Log(BallName + ": OnEnable");
    }
    
}
