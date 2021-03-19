using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class WaypointTactic : MonoBehaviour
{
    Transform prisonerBase;

    private Dictionary<Waypoint, List<Waypoint>> graph;
    Waypoint[] waypoints;

    GameObject[] castles;
    GameObject[] polices;
    GameObject[] prisoners;

   
    // Use this for initialization
    void Start()
    {
        graph = new Dictionary<Waypoint, List<Waypoint>>();
        waypoints = GameObject.FindObjectsOfType<Waypoint>();

        castles = GameObject.FindGameObjectsWithTag("Enemy Castle");
        polices = GameObject.FindGameObjectsWithTag("Police");
        prisoners = GameObject.FindGameObjectsWithTag("Prisoner");
        prisonerBase = GameObject.FindGameObjectWithTag("Enemy spawn position").transform;

        // Construct the graph
        ConstructGraph();

        updateTacticValue();
        //targetPoint = findTargetCastle();
    }

    // Update is called once per frame
    void Update()
    {
        //updateTacticValue();
        //targetPoint = findTargetCastle();
    }

    public void updateTacticValue()
    {
        valueOfOurUnitsNearby();
        foreach (var wp in waypoints)
        {
            wp.tacticWight = wp.GetTacticWightValue();
            //wp.toString();
        }
    }


    // Construct the graph
    public void ConstructGraph()
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
                    //Debug.DrawLine(w.transform.position, other.transform.position, Color.red, 100);
                }
            }
            graph.Add(w, edges);

            w.visibility = edges.Count;
            w.cover = w.checkCollision();
            w.distanceFromBase = Vector3.Distance(w.transform.position, prisonerBase.position);
            w.quality = w.GetQualityValue();
        }
    }


    public void valueOfOurUnitsNearby()
    {
        prisoners = GameObject.FindGameObjectsWithTag("Prisoner");
        foreach (var wp in waypoints)
        {
            float count = 0f;
            foreach (var p in prisoners)
            {
                count += Vector3.Distance(wp.transform.position, p.transform.position);
            }
            wp.ourUnitsNearby = count;
        }
    }


    public GameObject findTargetCastle()
    {
        castles = GameObject.FindGameObjectsWithTag("Enemy Castle");
        //Waypoint smallest = null;
        GameObject smallest = null;
        if (castles != null)
        {
            float min = Mathf.Infinity;
            for (int i = 0; i < castles.Length; i++)
            {
                Waypoint temp = FindSmallestTacticWeightWP(castles[i].transform);
                if (temp != null && temp.tacticWight < min)
                {
                    min = temp.tacticWight;
                    //smallest = temp;
                    smallest = castles[i];
                }
            }
        }
        return smallest;
    }


    public Waypoint FindSmallestTacticWeightWP(Transform trans)
    {
        SortedList<float, Waypoint> around = new SortedList<float, Waypoint>();
        foreach (var wp in waypoints)
        {
            float d = Vector3.Distance(wp.transform.position, trans.position);
            if (d < 20f)
            {
                around.Add(wp.tacticWight, wp);
            }
        }
        return around.Values[0];
    }


    public Waypoint FindSecuritySpot(Transform trans)
    {
        SortedList<float, Waypoint> around = new SortedList<float, Waypoint>();
        foreach (var wp in waypoints)
        {
            float d = Vector3.Distance(wp.transform.position, trans.position);
            if (d < 20f)
            {
                around.Add(wp.quality, wp);
            }
        }
        return around.Values[0];
    }


    // Find the nearest waypoint
    public Waypoint FindNearestWayPoint(Transform trans)
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
    public List<Waypoint> getTacticPath(Dictionary<Waypoint, List<Waypoint>> graph, Waypoint start, Waypoint goal)
    {
        // open list, the argument float means f_cost = g + h
        SortedList<float, Waypoint> openset = new SortedList<float, Waypoint>();
        // close list: <this node, parent node>
        Dictionary<Waypoint, Waypoint> closeset = new Dictionary<Waypoint, Waypoint>();
        // record the cost so far
        Dictionary<Waypoint, float> g = new Dictionary<Waypoint, float>();

        closeset.Add(start, null);
        g.Add(start, 0);
        openset.Add(start.GetTacticWightValue(Vector3.Distance(start.transform.position, goal.transform.position)), start);

        while (openset.Count > 0)
        {
            Waypoint current = openset.Values[0];
            openset.RemoveAt(0);

            if (current == goal)
                break;

            foreach (Waypoint next in graph[current])
            {
                float newG = g[current] + next.GetTacticWightValue(Vector3.Distance(next.transform.position, current.transform.position));
                if (!g.ContainsKey(next) || newG < g[next])
                {
                    if (openset.ContainsValue(next))
                    {
                        openset.RemoveAt(openset.IndexOfValue(next));
                    }
                    openset.Add(newG + goal.GetTacticWightValue(Vector3.Distance(next.transform.position, goal.transform.position)), next);

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
