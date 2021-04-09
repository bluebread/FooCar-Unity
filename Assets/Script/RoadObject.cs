using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class RoadObject
{
    private GameObject gameObject;
    private GameObject creator;

    public PathCreator pathCreator;

    public GameObject parentObject
    {
        set { gameObject.transform.parent = value.transform; }
    }
    public PhysicMaterial physicalMaterial
    {
        set
        {
            RoadMeshCreator meshCreator = creator.GetComponent<RoadMeshCreator>();
            meshCreator.SetPhysicalMaterial(value);
        }
    }

    List<Vector3> points;
    bool isClosed;
    PathSpace space;
    BezierPath.ControlMode mode;
    float roadWidth;
    List<float> roadMaxAngles;

    public RoadObject(List<Vector3> _points, List<float> roadMaxAngles, bool _isClosed, PathSpace _space, BezierPath.ControlMode _mode, float _roadWidth)
    {
        this.points = _points;
        this.isClosed = _isClosed;
        this.space = _space;
        this.mode = _mode;
        this.roadWidth = _roadWidth;
        this.roadMaxAngles = roadMaxAngles;

        gameObject = new GameObject("Road");
        CreatorInitialize();
    }

    private RoadObject() { }

    void CreatorInitialize()
    {
        creator = new GameObject("Road Creator");

        creator.transform.parent = gameObject.transform;
        creator.AddComponent<PathCreator>();
        creator.AddComponent<RoadMeshCreator>();

        creator.transform.localPosition = Vector3.zero;
        creator.transform.localRotation = Quaternion.identity;
        creator.transform.localScale = Vector3.one;

        PathInitialize();
        MeshInitialize();

        pathCreator = creator.GetComponent<PathCreator>();
    }

    void PathInitialize()
    {
        PathCreator pathCreator = creator.GetComponent<PathCreator>();

        pathCreator.bezierPath = new BezierPath(points, isClosed, space);
        pathCreator.bezierPath.ControlPointMode = mode;
        
        //pathCreator.bezierPath.ResetNormalAngles();
        for (int i = 0; i < pathCreator.bezierPath.NumAnchorPoints; i++)
                pathCreator.bezierPath.SetAnchorNormalAngle(i, this.roadMaxAngles[i]);
    }

    void MeshInitialize()
    {
        RoadMeshCreator meshCreator = creator.GetComponent<RoadMeshCreator>();
        meshCreator.pathCreator = creator.GetComponent<PathCreator>();
        meshCreator.roadWidth = this.roadWidth;
        meshCreator.ManualPathUpdated();
    }

    // public methods
    public List<Vector3> GetAnchors()
    {
        return points;
    }

    public static RoadObject Copy(RoadObject road)
    {
        RoadObject new_road = new RoadObject();

        new_road.points = road.points;
        new_road.isClosed = road.isClosed;
        new_road.space = road.space;
        new_road.mode = road.mode;
        new_road.gameObject = GameObject.Instantiate(road.gameObject);

        return new_road;
    }
    public void DestroyGameObject()
    {
        gameObject.SetActive(false);
        Object.Destroy(gameObject, 0.5f);
    }
}
