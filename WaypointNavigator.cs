using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaypointNavigator : MonoBehaviour
{
    public float distanceToTargetThreshold = 0.5f;
    public GameObject targetPoint;
    public float speed = 5;
    public float turnSpeed = 5;

    private Dictionary<Waypoint, List<Waypoint>> graph;
    private Waypoint currentWaypoint;
    private List<Waypoint> pathToTarget;
    Waypoint[] waypoints;


    // Use this for initialization
    void Start()
    {
        graph = new Dictionary<Waypoint, List<Waypoint>>();
        waypoints = GameObject.FindObjectsOfType<Waypoint>();

        Debug.Log(waypoints.Length);

        // Construct the graph
        ConstructGraph();

        // Find the nearest waypoint from NPC
        currentWaypoint = FindNearestWayPoint(transform);
        currentWaypoint.GetComponent<MeshRenderer>().material.color = Color.blue;
        Debug.DrawLine(transform.position, currentWaypoint.transform.position, Color.red, 1000);

        // Find the goal waypoint from the target
        Waypoint target = null;
        while (!target || target == currentWaypoint)
            target = FindNearestWayPoint(targetPoint.transform);
        target.GetComponent<MeshRenderer>().material.color = Color.red;

        // Calculates the path towards the goal waypoints
        pathToTarget = getPath(graph, currentWaypoint, target);

        // Draw the path
        for (int i = 0; i < pathToTarget.Count-1; i++)
        {
            Debug.DrawLine(pathToTarget[i].transform.position, pathToTarget[i+1].transform.position, Color.red, 1000);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, currentWaypoint.transform.position);

        if (distanceToTarget < distanceToTargetThreshold)
        {
            // If the waypoint has been reached, select next waypoint
            if (pathToTarget.Count == 0)
            {
                transform.position = currentWaypoint.transform.position;
            }
            currentWaypoint.GetComponent<MeshRenderer>().material.color = Color.black;
            currentWaypoint = pathToTarget[0];
            pathToTarget.RemoveAt(0);

            if (pathToTarget.Count > 0)//currentWaypoint != pathToTarget[pathToTarget.Count - 1])
                currentWaypoint.GetComponent<MeshRenderer>().material.color = Color.blue;
        }

        // NPC movement
        Vector3 direction = currentWaypoint.transform.position - transform.position;
        MoveToPoint(direction.normalized);
    }

    // NPC movement 
    void MoveToPoint(Vector3 moveDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    // Construct the graph
    void ConstructGraph()
    {
        foreach (Waypoint w in waypoints)
        {
            List<Waypoint> edges = new List<Waypoint>();

            // Generate the edges from this node
            foreach (Waypoint other in waypoints)
            {
                if (w != other && !Physics.Raycast(w.transform.position,
                        other.transform.position - w.transform.position,
                        Vector3.Distance(w.transform.position, other.transform.position)))
                {
                    edges.Add(other);
                    Debug.DrawLine(w.transform.position, other.transform.position, Color.red, 100);
                }
            }
            graph.Add(w, edges);
        }
    }

    // Find the nearest waypoint
    Waypoint FindNearestWayPoint(Transform trans)
    {
        Waypoint nearest = null;
        float minDistance = Mathf.Infinity;
        foreach (Waypoint w in waypoints)
        {
            if (!Physics.Raycast(trans.position, w.transform.position - trans.position, 
                Vector3.Distance(trans.position, w.transform.position))) 
            {
                float distance = Vector3.Distance(trans.position, w.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = w;
                }
            }
        }
        return nearest;
    }


    // Calculates the path towards the goal waypoints, using A* search.
    List<Waypoint> getPath(Dictionary<Waypoint, List<Waypoint>> graph, Waypoint start, Waypoint goal)
    {
        // open list, the argument float means f_cost = g + h
        SortedList<float, Waypoint> openset = new SortedList<float, Waypoint>();
        // close list: <this node, parent node>
        Dictionary<Waypoint, Waypoint> closeset = new Dictionary<Waypoint, Waypoint>();
        // record the cost so far
        Dictionary<Waypoint, float> g = new Dictionary<Waypoint, float>();                  

        closeset.Add(start, null);
        g.Add(start, 0);
        openset.Add(Vector3.Distance(start.transform.position, goal.transform.position), start);

        while (openset.Count > 0)
        {
            Waypoint current = openset.Values[0];
            openset.RemoveAt(0);

            if (current == goal)
                break;

            foreach (Waypoint next in graph[current])
            {
                float newG = g[current] + Vector3.Distance(next.transform.position, current.transform.position);
                if (!g.ContainsKey(next) || newG < g[next])
                {
                    if (openset.ContainsValue(next))
                    {
                        openset.RemoveAt(openset.IndexOfValue(next));
                    }
                    openset.Add(newG + Vector3.Distance(next.transform.position, goal.transform.position), next);

                    if (closeset.ContainsKey(next))
                    {
                        closeset.Remove(next);
                    }
                    closeset.Add(next, current);

                    if (g.ContainsKey(next))
                    {
                        g.Remove(next);
                    }
                    g.Add(next, newG);
                }
            }
        }

        // Return the path to the goal
        List<Waypoint> path = new List<Waypoint>();
        Waypoint w = goal;
        path.Add(goal);
        while (closeset[w] != null)
        {
            path.Add(closeset[w]);
            w = closeset[w];
        }
        path.Reverse();
        return path;
    }
}
