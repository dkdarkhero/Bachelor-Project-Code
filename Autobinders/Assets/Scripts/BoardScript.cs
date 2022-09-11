using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
//using UnityEditor.AI;
using System.Threading;

public class BoardScript : NetworkBehaviour
{
    [SerializeField]
    List<GameObject> whiteTiles = new List<GameObject>();
    [SerializeField]
    List<GameObject> blackTiles = new List<GameObject>();
    
    public PlayTile[][] tiles = new PlayTile[8][];
    public List<RosterPillar> pillars = new List<RosterPillar>();
    public List<GameObject> activeUnits = new List<GameObject>();
    public List<GameObject> storedUnits = new List<GameObject>();
    public List<GameObject> friendlyUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    const float OFFSET_TILE = 0.1f;
    const float OFFSET_START = 1.4f;

    GameObject flag;
    GameObject gameManager;
    float timer = 0;

    bool boardChanged = false;
    float boardTimer = 0;


    public override void OnNetworkSpawn()
    {
       //if (IsOwner == true)
       // {
       //     SetBoardPosServerRpc(NetworkManager.LocalClientId);
       // }

        if (IsServer == true)
        {
            Position.Value = transform.position += new Vector3(NetworkManager.ConnectedClientsIds.Count * 30, 0, 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bool lastTileBlack = false;
        MeshRenderer boardMesh = gameObject.GetComponent<MeshRenderer>();
        Vector3 tileSize = whiteTiles[0].GetComponent<MeshRenderer>().bounds.size;

        for (int i = 0; i < 8; i++)
        {
            tiles[i] = new PlayTile[8];
            for (int j = 0; j < 8; j++)
            {
                Vector3 nextPos = new Vector3(
                    transform.position.x + (tileSize.x + OFFSET_TILE) * i - boardMesh.bounds.size.x * 0.5f + tileSize.x * 0.5f + OFFSET_START,
                    transform.position.y + 0.5f,
                    transform.position.z + (tileSize.z + OFFSET_TILE) * j - boardMesh.bounds.size.z * 0.5f + tileSize.z * 0.5f + OFFSET_START);

                if (lastTileBlack) //place a white tile
                {
                    tiles[i][j] = new PlayTile(Instantiate(whiteTiles[Random.Range(0, 8)], nextPos, default, gameObject.transform));
                }
                else //place a black tile
                {
                    tiles[i][j] = new PlayTile(Instantiate(blackTiles[Random.Range(0, 8)], nextPos, default, gameObject.transform));
                }

                lastTileBlack = !lastTileBlack; //inverts the bool
            }
            lastTileBlack = !lastTileBlack; //inverts the bool
        }

        //NavMeshBuilder.ClearAllNavMeshes(); //clears all navmeshes
        //NavMeshBuilder.BuildNavMesh(); //creates a navmesh for all walkable objects


        //get a reference to all the placePillar child objects and the flag
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name.Contains("placePillar"))
            {
                pillars.Add(new RosterPillar(transform.GetChild(i).gameObject));
            }
            else if (transform.GetChild(i).name.Contains("Flagpole"))
            {
                flag = transform.GetChild(i).transform.GetChild(0).gameObject;
            }
            else if (transform.GetChild(i).name.Contains("WalkingPlane"))
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
        }

        //sort the list
        pillars.Sort(delegate(RosterPillar x, RosterPillar y)
        {
            return x.pillar.name.CompareTo(y.pillar.name);
        }); 

        gameManager = GameObject.Find("GameManager"); //get a reference to the GameManager
    }

    // Update is called once per frame
    void Update()
    {
        //if (boardChanged == false && Position.Value != Vector3.zero && gameObject.transform.position != Position.Value)
        //{
        //    boardChanged = true;
        //    gameObject.transform.position = Position.Value;
        //}

        if (Position.Value != transform.position)
        {
            transform.position = Position.Value;
        }
        #region flag color test
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); //random color
            flag.GetComponent<MeshRenderer>().materials[0].color = color; //set flag color
            timer = 1;
        }
        #endregion

        if (gameManager.GetComponent<GameManager>().state == GameManager.GameState.Fight)
        {
            foreach (GameObject unit in friendlyUnits)
            {
                if (unit.GetComponent<UnitScript>().enemyTarget == null && enemyUnits.Count > 0)
                {
                    unit.GetComponent<UnitScript>().ChoseTarget(enemyUnits);
                }
            }
            foreach (GameObject unit in enemyUnits)
            {
                if (unit.GetComponent<UnitScript>().enemyTarget == null && friendlyUnits.Count > 0)
                {
                    unit.GetComponent<UnitScript>().ChoseTarget(friendlyUnits);
                }
            }
        }
    }

    public class RosterPillar
    {
        public GameObject unit;
        public readonly GameObject pillar;

        public RosterPillar(GameObject pillar)
        {
            this.pillar = pillar;
        }
    }

    public class PlayTile
    {
        public GameObject unit;
        public GameObject tile; //pls don't assign to this outside of the Start() function of BoardScript

        public PlayTile(GameObject tile)
        {
            this.tile = tile;
        }

    }

    [ServerRpc]
    public void SetBoardPosServerRpc(ulong index )
    {
        Position.Value = transform.position + new Vector3(30 * index,0,0);
    }


}
