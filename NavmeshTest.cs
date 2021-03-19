//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class NavmeshTest : MonoBehaviour
//{
//    private NavMeshAgent agent;
//    Transform target;
//    WaypointTactic wt;

//    // Start is called before the first frame update
//    void Start()
//    {
//        agent = gameObject.GetComponent<NavMeshAgent>();
//        wt = GetComponent<WaypointTactic>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        target = wt.findTargetCastle().transform;

//        if (Vector3.Distance(transform.position, target.position) < 10f)
//        {
//            target = wt.FindSecuritySpot(transform).transform;
//            //Debug.Log("-----------" + target);
//        }
//        agent.SetDestination(target.position);
//    }
//}
