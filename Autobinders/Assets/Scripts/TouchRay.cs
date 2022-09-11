using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TouchRay : NetworkBehaviour
{
    [SerializeField]
    GameObject plane; //used for height calculations
    [SerializeField]
    GameObject board; //used to get information about the board
    [SerializeField]
    GameObject gameManager;

    private GameObject draggedUnit;
    private Vector3 draggedUnitStartPos;

    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsServer == false && IsOwner == true)
        {
            GameObject.FindWithTag("ServerCam").SetActive(false);
        }

        if (IsOwner == false && IsServer == false)
        {
            gameObject.SetActive(false);
        }

        if (IsServer == true)
        {
            gameObject.GetComponent<Camera>().enabled = false;
            gameObject.GetComponent<AudioListener>().enabled = false;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.GetComponent<GameManager>().state == GameManager.GameState.Manage)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Construct a ray from the current mouse coordinates
                Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                Debug.Log($"Camera Owned By {OwnerClientId}");

                Physics.Raycast(ray, out RaycastHit hit);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 30);

                if (hit.collider.gameObject != null)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.gameObject.CompareTag("Unit"))
                    {
                        draggedUnitStartPos = hit.collider.gameObject.transform.position;
                        draggedUnit = hit.collider.gameObject;
                        draggedUnit.GetComponent<NavMeshAgent>().enabled = false;
                        draggedUnit.GetComponent<UnitScript>().beingDragged = true;
                    }
                    else if (hit.collider.gameObject.CompareTag("ShopUnit")) //will probably be handled entirely through a script on the UI
                    {
                        //buy unit method call
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && draggedUnit != null)
            {
                // Construct a ray from the current mouse coordinates
                Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                RaycastHit[] hits = Physics.RaycastAll(ray);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 30);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.CompareTag("Tile")) //move to tile
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                if (board.GetComponent<BoardScript>().tiles[x][y].tile == hit.collider.gameObject)
                                {
                                    RequestChangeTileServerRpc(x,y, draggedUnit.GetComponent<UnitScript>().NetworkObjectId);
                                }
                            }
                        }

                       
                    }
                    else if (hit.collider.gameObject.CompareTag("RosterPillar")) //move to roster pillar
                    { 
                        for (int x = 0; x < 8; x++)
                        {
                            if (board.GetComponent<BoardScript>().pillars[x].pillar == hit.collider.gameObject)
                            {
                                RequestPillarServerRpc(x, draggedUnit.GetComponent<UnitScript>().NetworkObjectId);
                                
                            }
                        }
                       
                    }
                }

                ////if the object failed to place, reset its position
                //if (!placed)
                //{
                //    draggedUnit.transform.position = draggedUnitStartPos;
                //}

                draggedUnit.GetComponent<UnitScript>().beingDragged = false;
                draggedUnit = null;
            }
            else if (draggedUnit != null)
            {
                // Construct a ray from the current mouse coordinates
                Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                RaycastHit[] hits = Physics.RaycastAll(ray);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 30);

                foreach (RaycastHit hit in hits)
                {
                    if (draggedUnit != null)
                    {
                        draggedUnit.transform.position = new Vector3(
                            hit.point.x,
                            plane.transform.position.y + draggedUnit.GetComponent<MeshCollider>().bounds.size.y * 0.5f,
                            hit.point.z);
                    }
                }
            }
        }
    }




    public void RequestChangeTile(int tileX, int tileY, ulong ID)
    {
        GameObject[] unitArray = GameObject.FindGameObjectsWithTag("Unit");
        GameObject unit = new GameObject();

        foreach (GameObject item in unitArray)
        {
            if (item.GetComponent<UnitScript>().NetworkObjectId == ID)
            {
                unit = item;
            }
        }

        
        GameObject hitObject = board.GetComponent<BoardScript>().tiles[tileX][tileY].tile;
        bool friendlyTile = false;

        //ensure that the tile hit is one from the bottom 4 rows
        for (int i = 0; i < board.GetComponent<BoardScript>().tiles.Length; i++) //columns
        {
            for (int j = 0; j < board.GetComponent<BoardScript>().tiles[i].Length; j++) //rows
            {
                if (board.GetComponent<BoardScript>().tiles[i][j].tile == hitObject)
                {
                    if (board.GetComponent<BoardScript>().tiles[i][j].unit != null)
                    {
                        break;
                    }

                    if (j < 4) //lower 4 rows
                    {
                        if (unit.GetComponent<UnitScript>().tile != null)
                        {
                            unit.GetComponent<UnitScript>().tile.unit = null;
                        }
                        board.GetComponent<BoardScript>().tiles[i][j].unit = unit;
                        unit.GetComponent<UnitScript>().tile = board.GetComponent<BoardScript>().tiles[i][j];
                        friendlyTile = true;
                    }
                    break;
                }
            }

            if (friendlyTile)
            {
                break;
            }
        }

        if (friendlyTile)
        {
            unit.GetComponent<UnitScript>().Position.Value = new Vector3(
            hitObject.transform.position.x,
            plane.transform.position.y + unit.GetComponent<MeshCollider>().bounds.size.y * 0.5f,
            hitObject.transform.position.z);

            //add to active units if it's not there
            if (!board.GetComponent<BoardScript>().activeUnits.Contains(unit))
            {
                board.GetComponent<BoardScript>().storedUnits.Remove(unit);
                board.GetComponent<BoardScript>().activeUnits.Add(unit);
            }

            try
            {
                //unit.GetComponent<NavMeshAgent>().enabled = true;
                unit.GetComponent<PathFindingAgent>().waypoint = unit.transform.position;
            }
            catch (System.Exception) { }

            //if moved from a pillar
            if (unit.GetComponent<UnitScript>().pillar != null)
            {
                unit.GetComponent<UnitScript>().pillar.unit = null;
                unit.GetComponent<UnitScript>().pillar = null;
            }


           
          
        }
    }

    [ServerRpc]
    public void RequestChangeTileServerRpc(int tileX, int tileY, ulong ID)
    {
        RequestChangeTile(tileX,tileY, ID);
    }

    [ServerRpc]
    public void RequestPillarServerRpc(int index, ulong ID)
    {
        RequestPillar(index,ID);
    }

    public void RequestPillar(int index, ulong ID)
    {
        GameObject hitObject = board.GetComponent<BoardScript>().pillars[index].pillar;

        GameObject[] unitArray = GameObject.FindGameObjectsWithTag("Unit");
        GameObject unit = new GameObject();

        foreach (GameObject item in unitArray)
        {
            if (item.GetComponent<UnitScript>().NetworkObjectId == ID)
            {
                unit = item;
            }
        }
        //add to stored units if it's not there
        if (!board.GetComponent<BoardScript>().storedUnits.Contains(unit))
        {
            board.GetComponent<BoardScript>().activeUnits.Remove(unit);
            board.GetComponent<BoardScript>().storedUnits.Add(unit);
        }

        //ensure the pillar is empty
        foreach (BoardScript.RosterPillar pillar in board.GetComponent<BoardScript>().pillars)
        {
            if (pillar.pillar == hitObject && pillar.unit == null)
            {
                //if moved from one pillar to another
                if (unit.GetComponent<UnitScript>().pillar != null)
                {
                    unit.GetComponent<UnitScript>().pillar.unit = null;
                }

                pillar.unit = unit;
                unit.GetComponent<UnitScript>().pillar = pillar;

                //if moved from a tile
                if (unit.GetComponent<UnitScript>().tile != null)
                {
                    unit.GetComponent<UnitScript>().tile.unit = null;
                    unit.GetComponent<UnitScript>().tile = null;
                }

                Vector3 tempPos = new Vector3(
                    hitObject.transform.position.x,
                    plane.transform.position.y + unit.GetComponent<MeshCollider>().bounds.size.y * 0.5f,
                    hitObject.transform.position.z);

                unit.GetComponent<UnitScript>().Position.Value = tempPos;
                break;
            }
        }
    }



}
