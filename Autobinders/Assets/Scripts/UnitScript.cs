using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitScript : NetworkBehaviour
{
    public float health = 100;
    public float damage = 1;
    public float attackSpeed = 1;
    public float attackRange = 1;
    public bool beingDragged = false;
    public bool updatePos = true;
    public GameObject enemyTarget;
    public BoardScript.RosterPillar pillar;
    public BoardScript.PlayTile tile;
    public Vector3 lastPos;

    public Vector3 shownNetworkPos;
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<bool> startingPosSet = new NetworkVariable<bool>();

    float attackTimer = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer == true)
        {
            Position.Value = transform.position;
            startingPosSet.Value = true;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        attackTimer = attackSpeed;

        if (IsOwner == true)
        {
            SetPositionServerRpc();
        }

        Debug.Log($"owner {IsOwner}");
        Debug.Log($"client {IsClient}");
        Debug.Log($"Unit ID {OwnerClientId}");
        Debug.Log($"local ID {NetworkManager.LocalClientId}");
    }

    // Update is called once per frame
    void Update()
    {
        if (startingPosSet.Value == true)
        {
            if (beingDragged == false)
            {
                transform.position = Position.Value;
            }
        }
     
        attackTimer -= Time.deltaTime;
        shownNetworkPos = Position.Value;

        //check if the unit has moved while on a pillar, if yes, tell the pillar that no unit is standing on it
        //this is a horrible way to do this, but I couldn't figure out a better way using events or properties :(
        if (pillar != null && !beingDragged) 
        {
            if (lastPos != null && lastPos != transform.position)
            {
                pillar.unit = null;
                pillar = null;
            }

            lastPos = transform.position;
        }
        else if (tile != null && !beingDragged)
        {
            if (lastPos != null && lastPos != transform.position)
            {
                tile.unit = null;
                tile = null;
            }

            lastPos = transform.position;
        }
        else if (enemyTarget != null && health > 0) //if the unit has a target
        {
            gameObject.GetComponent<PathFindingAgent>().waypoint = enemyTarget.transform.position;

            //ready to attack
            if (attackTimer <= 0)
            {
                enemyTarget.GetComponent<UnitScript>().health -= damage;
                attackTimer = attackSpeed;
            }

            //enemy is dead
            if (enemyTarget.GetComponent<UnitScript>().health <= 0)
            {
                enemyTarget = null;
            }
        }
    }

    public void ChoseTarget(List<GameObject> enemies)
    {
        GameObject target = enemies[0];

        foreach (GameObject enemy in enemies)
        {
            //if enemy is closer than target
            if (Vector3.Distance(enemy.transform.position, gameObject.transform.position) < Vector3.Distance(target.transform.position, gameObject.transform.position) &&
                enemy.GetComponent<UnitScript>().health > 0)
            {
                target = enemy;
            }
        }

        enemyTarget = target;
    }


    public void CallTileChange(Vector3 pos)
    {
        ChangeTileServerRpc(pos);
    }

    [ServerRpc]
    public void ChangeTileServerRpc(Vector3 pos)
    {
        ChangeTile(pos);
        
    }

    [ServerRpc]
    public void SetPositionServerRpc()
    {
        Position.Value = transform.position;
    }

    [ServerRpc]
    public void DebugServerRpc()
    {
        Debug.Log("CAKE");
    }
    [ClientRpc]
    public void SetPositionClientRpc()
    {
        transform.position = Position.Value;
    }

    public void ChangeTile(Vector3 newPos)
    {
        Position.Value = newPos;
        transform.position = Position.Value;
        SetPositionClientRpc();
    }

    [ServerRpc]
    public void SubmitSpawnRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("ServerCall");
    }
}
