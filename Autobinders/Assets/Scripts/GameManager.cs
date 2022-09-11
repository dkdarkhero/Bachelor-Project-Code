using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    const float FIGHTTIME = 30;
    const float MANAGETIME = 30;
    const float TRANSITIONTIME = 5;

    public enum GameState
    {
        Manage,
        Fight,
        TransitionToManage,
        TransitionToFight
    }
    public GameState state = GameState.Manage;
    public GameState nextState;
    public float roundTimer = 1;
    public bool paused = true;
    public List<GameObject> boards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //state = GameState.Transition;
        nextState = GameState.Manage;
        roundTimer = 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
        {
            return;
        }

        roundTimer -= Time.deltaTime;

        if (roundTimer <= 0)
        {
            state = nextState;
            switch (nextState)
            {
                case GameState.Manage:
                    nextState = GameState.TransitionToFight;
                    roundTimer = MANAGETIME;
                    break;
                case GameState.Fight:
                    nextState = GameState.TransitionToManage;
                    roundTimer = FIGHTTIME;
                    break;
                case GameState.TransitionToManage:
                    nextState = GameState.Manage;
                    roundTimer = TRANSITIONTIME;
                    break;
                case GameState.TransitionToFight:
                    nextState = GameState.Fight;
                    roundTimer = TRANSITIONTIME;
                    break;
            }

            if (state == GameState.Fight)
            {
                SelectBattles();
            }
        }
    }

    void SelectBattles()
    {
        if (boards.Count > 1)
        {
            List<GameObject> fighters = boards;

            while (fighters.Count > 0)
            {
                int fighterIndexFirst = Random.Range(0, fighters.Count);
                int fighterIndexSecond = Random.Range(0, fighters.Count);

                if (fighterIndexFirst != fighterIndexSecond)
                {
                    //fighter one should fight fighter two
                    GameObject fighterOne = fighters[fighterIndexFirst];
                    GameObject fighterTwo = fighters[fighterIndexSecond];
                    SetupBoardForFight(fighterOne, fighterTwo);
                    fighters.Remove(fighterOne);
                    fighters.Remove(fighterTwo);
                }
                else if (fighters.Count == 1)
                {
                    int lastFighterIndex = boards.IndexOf(fighters[0]);
                    bool resolved = false;
                    while (!resolved)
                    {
                        int ghostFighterIndex = Random.Range(0, boards.Count);
                        if (ghostFighterIndex != lastFighterIndex)
                        {
                            //last fighter should fight ghost fighter which is a copy of a random player. if the ghost loses the player it copies doesn't lose any lives
                            GameObject lastFighter = fighters[0];
                            GameObject ghostFighter = boards[ghostFighterIndex];
                            SetupBoardForFight(lastFighter, ghostFighter);
                            resolved = true;
                            fighters.Clear();
                        }
                    }
                }
            }
        }
    }

    void SetupBoardForFight(GameObject first, GameObject second)
    {
        first.GetComponent<BoardScript>().friendlyUnits = first.GetComponent<BoardScript>().activeUnits;
        first.GetComponent<BoardScript>().enemyUnits = second.GetComponent<BoardScript>().activeUnits;

        int boardSize = second.GetComponent<BoardScript>().tiles.Length; //should be 8

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < second.GetComponent<BoardScript>().tiles[i].Length; j++)
            {
                first.GetComponent<BoardScript>().tiles[boardSize - i - 1][boardSize - j - 1].unit = second.GetComponent<BoardScript>().tiles[i][j].unit;
            }
        }
    }
}
