using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rasul.Bezier;


public class StreetController : MonoBehaviour {

    private StreetWalker[] walkers;
    private float[] walkerWaitTimes;
    [SerializeField, HideInInspector]
    private Vector3[] waypoints;

    [SerializeField]
    private float walkerSpawnTime = 5f;
    private float spawnHitHeight = 1f;
    private int spawnIndex;
    [SerializeField]
    private int 
        walkersCount,
        staticCarsCount;
    [SerializeField]
    private GameObject 
        road,
        carsHolder,
        walkerMale,
        walkerFemale,
        staticCar;

    private void OnValidate()
    {
        // get road data
        if (waypoints == null || waypoints.Length <= 0)
        {
            RoadCreater roadCreater = road.GetComponent<RoadCreater>();
            Vector3[] points = road.GetComponent<PathCreater>().path.GetEvenlySpacedPoints(roadCreater.spacing);
            bool isClosed = road.GetComponent<PathCreater>().path.IsClosed;
            float roadWidth = roadCreater.roadWidth;
            // declare and init using variables
            int index = 0;
            Vector3 forward;
            waypoints = new Vector3[points.Length * 2];
            float height = points[0].y;

            // go throw each road point to find relative points on right and left side
            for (int i = 0; i < points.Length; i++)
            {
                forward = Vector3.zero;
                if (i < points.Length - 1 || isClosed)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }
                if (i > 0 || isClosed)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }
                forward.y = height;
                forward.Normalize();
                Vector3 left = new Vector3(-forward.z, height, forward.x);
                waypoints[index] = points[i] + left * (roadWidth + 3) * 0.5f;
                waypoints[index + 1] = points[i] - left * (roadWidth + 3) * 0.5f;
                index += 2;
            }
        }
    }

    private void Start()
    {
        // decalre variables using to spwan walkers
        StreetWalker walker;
        walkers = new StreetWalker[walkersCount];
        walkerWaitTimes = new float[walkersCount];

        // spawn walkers
        for (int i = 0; i < walkersCount; i++)
        {
            // spawn male or female ?
            if (i % 2 == 0)
            {
                walker = Instantiate(walkerMale).GetComponent<StreetWalker>();
            }
            else
            {
                walker = Instantiate(walkerFemale).GetComponent<StreetWalker>();
            }
            walkers[i] = walker;
            // also init wait time for each walker
            walkerWaitTimes[i] = walkerSpawnTime * i;
        }

        // declare variables using in static cars spawning
        int waypointsCount = waypoints.Length,
            spawnCount = waypoints.Length / staticCarsCount;
        Vector3
            leftPoint, 
            rightPoint,
            direction,
            position;
        int index;

        //spawn static cars
        for (int i = 0; i < staticCarsCount; i++)
        {
            index = spawnCount * i;
            leftPoint   = waypoints[index % waypointsCount];
            rightPoint  = waypoints[(index + 1) % waypointsCount];
            direction   = leftPoint - waypoints[(index + 2) % waypointsCount];
            position = Vector3.Lerp(leftPoint, rightPoint, Random.value);
            position += road.transform.position;
            position.y = 2;
            
            SpawnStaticCar(position, position + direction).parent = carsHolder.transform;
        }
    }

    private void FixedUpdate()
    {
        StreetWalker walker;
        for(int i = 0; i < walkersCount; i++)
        {
            walker = walkers[i];

            if (walker.Active)
            {
                walker.Walk();
            }
            else
            {
                if (walkerWaitTimes[i] <= 0)
                {
                    walkerWaitTimes[i] = walkerSpawnTime;
                }
                else
                {
                    walkerWaitTimes[i] -= Time.deltaTime;
                    if (walkerWaitTimes[i] <= 0)
                    {
                        SetWalker(walker);
                    }
                }
            }
        }
    }
    
    private void SetWalker(StreetWalker walker)
    {
        int waypointsCount = waypoints.Length;
        int count = waypointsCount / walkersCount;
        spawnIndex += count % waypointsCount;
        Vector3 
            start   = road.transform.position + waypoints[spawnIndex % waypointsCount],
            end     = road.transform.position + waypoints[(spawnIndex + 1) % waypointsCount];
        float distance = Vector3.Distance(start,end);
        RaycastHit hit;
        start.y = spawnHitHeight;
        end.y = spawnHitHeight;

        while (Physics.Raycast(start, end - start, out hit,distance))
        {
            spawnIndex++;
            start = road.transform.position + waypoints[spawnIndex % waypointsCount];
            end = road.transform.position + waypoints[(spawnIndex + 1) % waypointsCount];
            start.y = spawnHitHeight;
            end.y = spawnHitHeight;
        }

        spawnIndex++;
        start = road.transform.position + waypoints[spawnIndex % waypointsCount];
        end = road.transform.position + waypoints[(spawnIndex + 1) % waypointsCount];
        start.y = road.transform.position.y;
        end.y = start.y;
        walker.Init(start, end);
    }
#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] bool showAvailablePathes = false;
    [SerializeField] Color darkBlue;
    [SerializeField] Color lightBlue;
    private void OnDrawGizmos()
    {
        if (showAvailablePathes && waypoints!=null && waypoints.Length > 0)
        {
            bool carIsNear = false;
            float distance = Vector3.Distance(waypoints[0], waypoints[1]);
            Vector3 start, end;
            
            for (int i = 0; i < waypoints.Length; i += 2)
            {
                start = road.transform.position + waypoints[i % waypoints.Length];
                start.y = spawnHitHeight;
                
                end = road.transform.position + waypoints[(i + 1) % waypoints.Length];
                end.y = spawnHitHeight;

                if (Physics.Raycast(start, end - start, distance))
                {
                    if(!carIsNear) carIsNear = true;
                    Gizmos.color = darkBlue;
                }
                else
                {
                    if (carIsNear)
                    {
                        Gizmos.color = darkBlue;
                        carIsNear = false;
                    }
                    else
                    {
                        Gizmos.color = lightBlue;
                    }
                }
                Gizmos.DrawLine(start, end);
            }
        }
    }
#endif

    private Transform SpawnStaticCar(Vector3 position, Vector3 forward)
    {
        var car = Instantiate(staticCar).transform;
        car.position = position;
        car.LookAt(forward);
        return car;
    }
}
