using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;

public class PathFindingAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    public Vector3 waypoint;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (agent != null && agent.enabled)
        {
            agent.SetDestination(waypoint);
        }
    }
}
