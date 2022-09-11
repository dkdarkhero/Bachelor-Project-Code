using HelloWorld;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManagerScript : NetworkBehaviour
{
    public GameObject player;
    public List<GameObject> unitList = new List<GameObject>();
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnUnitButton()
    {
        SubmitSpawnRequestServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc]
    void SubmitSpawnRequestServerRpc(ulong id)
    {
        SpawnUnitServer(id);
    }

    [ServerRpc]
    void SubmitMoveRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        MoveUnits();
    }

    public void SpawnUnitServer(ulong clientID)
    {
        //debug
        GameObject go = Instantiate(player,new Vector3(-19f, 1.74000001f, 9.72999954f), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<NetworkObject>().ChangeOwnership(clientID);
        unitList.Add(go);
        Debug.Log(unitList.Count);
    }

    public void MoveUnits()
    {
        foreach (GameObject unit in unitList)
        {
            unit.GetComponent<HelloWorldPlayer>().Move();
        }

        Debug.Log($"moved {unitList.Count}");
    }

    public void MoveUnitsButton()
    {
        SubmitMoveRequestServerRpc();
    }
}
