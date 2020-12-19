using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{
    public Tile[,] tiles;
    public bool turn = true;

    public Transform tilePrefab;
    public Material selectedMat;
    public Material standardMat;
    public Material aoeMat;
    public Material moveMat;
    TurnOptions turnType;

    public ArrayList friendlies;
    public ArrayList enemies;

    public ArrayList aoeTiles;
    public Tile currentSelected;
    public AI ai;

    public int rows = 5;
    public int cols = 5;

    int numberOfKills = 0;
    public Text kills;
    public Text aiKills;
    public Text AIText;
    public Text winText;
    public Text loseText;

    //option buttons
    public Button m_Move, m_Attack, m_Place;
    
    public enum TurnOptions
    {
        Move,
        Attack,
        Place,
        None
    }

    void Start()
    {
        tiles = initBoardState();
        aoeTiles = new ArrayList();
        friendlies = new ArrayList();
        enemies = new ArrayList();

        turnType = TurnOptions.None;
        m_Move.onClick.AddListener(MoveListener);
        m_Place.onClick.AddListener(PlaceListener);
        m_Attack.onClick.AddListener(AttackListener);

    }

    public void MoveListener()
    {
        this.SetTurn(TurnOptions.Move);
    }

    public void PlaceListener()
    {
        this.SetTurn(TurnOptions.Place);
    }

    public void AttackListener()
    {
        this.SetTurn(TurnOptions.Attack);

        if (friendlies.Contains(currentSelected))
        {
            foreach (Tile tile in aoeTiles)
            {
                if (tile.getUnit() != null)
                {
                    Destroy(tile.getUnit().gameObject);

                    if (enemies.Contains(tile))
                    {
                        enemies.Remove(tile);
                        numberOfKills += 1;
                    }

                    if (friendlies.Contains(tile)) {
                        enemies.Remove(tile);
                        numberOfKills -= 1;
                    }

                    tile.setUnit(null);
                }
            }
        }
        turn = !turn;
        this.SetTurn(TurnOptions.None);
    }

    public void SetTurn(TurnOptions turnType)
    {
        this.turnType = turnType;
    }

    // Update is called once per frame
    void Update()
    {

        UnitAoECheck();
        if (turnType != TurnOptions.Move)
        {
            mouseToBoard();
        }


        if (turnType != TurnOptions.None)
        {
            switch (turnType) {
                case TurnOptions.Place:
                    mouseToSelect();
                    break;
                case TurnOptions.Move:
                    Move(currentSelected);
                    break;
                default:
                    break;
            }
        }

        if (turn != true) {
            turnType = TurnOptions.None;
            int enemyTurnType = UnityEngine.Random.Range(0, 3);
            if (enemyTurnType == 0 && enemies.Count > 0) //move
            {
                if ((friendlies.Count + enemies.Count) != (cols * rows))
                {
                    AIText.text = "The AI Moved";
                    ai.movePiece();
                }
            } 
            else if (enemyTurnType == 1 && enemies.Count > 0) //attack
            {
                AIText.text = "The AI Attacked";
                ai.Attack();
            } 
            else if (enemyTurnType == 2)
            {
                AIText.text = "The AI Placed An Enemy";
                ai.makeMove(tiles);
            }
            else //place
            {
                AIText.text = "The AI Placed An Enemy";
                ai.makeMove(tiles);
            }

        }

        if (numberOfKills >= 10)
        {
            winText.enabled = true;
        }
        else if (numberOfKills >= 10 && ai.numberOfKills >= 10)
        {
            winText.enabled = true;
        } 
        else if (ai.numberOfKills >= 10 && numberOfKills < 10)
        {
            loseText.enabled = true;
        }

        aiKills.text = "AI Kills: " + ai.numberOfKills;
        kills.text = "Personal Kills: " + numberOfKills;
    }

    Tile[,] initBoardState()
    {
        Tile[,] tiles = new Tile[rows, cols];

        for(int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Transform tileTransform = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                Tile tile = tileTransform.gameObject.AddComponent<Tile>();
                tile.setX(x);
                tile.setY(y);
                tile.setMat(standardMat);
                tile.transform.parent = this.transform;
                tile.transform.gameObject.layer = 9;
                tiles[x,y] = tile;
            }
        }

        return tiles;
    }

    void mouseToBoard()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Tile");

        RaycastHit hit = new RaycastHit();

        //raycast from the mouse cursor to the plane
        if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, layerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (currentSelected != null)
                {
                    currentSelected.setMat(standardMat);
                    if (aoeTiles.Count != 0)
                    {
                        foreach (Tile tile in aoeTiles)
                        {
                            tile.setMat(standardMat);
                        }
                    }

                    aoeTiles.Clear();
                }

                currentSelected = hit.transform.GetComponent<Tile>();
                currentSelected.setMat(selectedMat);


            }

        }
    }

    void Move(Tile tile)
    {
        if (tile.getUnit() != null)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = LayerMask.GetMask("Tile");

            RaycastHit hit = new RaycastHit();
            Tile moveTarget = null;

            ArrayList neighbors = this.getNeighbors(tile);

            foreach (Tile t in neighbors)
            {
                if (t.getUnit() == null)
                {
                    t.setMat(moveMat);
                }
            }

            //raycast from the mouse cursor to the plane
            if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, layerMask))
            {
                Transform prefab = tile.getUnit().prefab;

                if (Input.GetMouseButtonDown(0))
                {
                    moveTarget = hit.transform.GetComponent<Tile>();

                    bool isPossibleMove = false;
                    
                    foreach (Tile t in neighbors)
                    {
                        if (moveTarget == t && t.getUnit() == null)
                        { 
                            isPossibleMove = true;
                            break;
                        }
                    }

                    if (moveTarget.getUnit() == null && isPossibleMove)
                    {
                        Destroy(tile.getUnit().gameObject);
                        friendlies.Remove(tile);
                        tile.setUnit(null);
                        tile.setMat(standardMat);
                        currentSelected = moveTarget;
                        spawnUnit(prefab);
                        currentSelected.setMat(selectedMat);
                        turnType = TurnOptions.None;

                    }

                    foreach (Tile t in neighbors)
                    {
                        t.setMat(standardMat);
                    }
                    UnitAoECheck();
                    mouseToBoard();



                }
            }
        }
    }

    void mouseToSelect()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("UnitSelect");

        RaycastHit hit = new RaycastHit();

        //raycast from the mouse cursor to the plane
        if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, layerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.spawnUnit(hit.transform.GetComponent<Unit>().prefab);
            }
        }
    }

    Unit mouseToUnit()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Unit");

        RaycastHit hit = new RaycastHit();

        //raycast from the mouse cursor to the plane
        if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, layerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                return hit.transform.GetComponent<Unit>();
            }
            else return null;
        }

        else return null;
    }

    public void spawnUnit(Transform unitPrefab)
    {
        
        if (turn == true && currentSelected != null && currentSelected.getUnit() == null)
        {
            Transform unit = Instantiate(unitPrefab, new Vector3(currentSelected.getX(), unitPrefab.position.y, currentSelected.getY()), Quaternion.identity);
            currentSelected.setUnit(unit.GetComponent<Unit>());
            friendlies.Add(currentSelected);
            turn = !turn;
        }

        else if (turn == false)
        {
            Transform unit = Instantiate(unitPrefab, new Vector3(ai.tile.getX(), unitPrefab.position.y, ai.tile.getY()), Quaternion.identity);

            Vector3 rot = unit.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);
            unit.rotation = Quaternion.Euler(rot);
            ai.tile.setUnit(unit.GetComponent<Unit>());
            enemies.Add(ai.tile);
            turn = !turn;
        }

        
    }

    public void UnitAoECheck()
    {
        if (currentSelected != null)
        {
            if (currentSelected.getUnit() != null)
            {
                int[] AoE = currentSelected.getUnit().getAoE();
                displayAoE(AoE);
            }
        }
    }

    public void displayAoE(int[] AoE)
    {
        int x = currentSelected.getX();
        int y = currentSelected.getY();

        for (int j = 0; j < AoE.Length; j++)
        {
            if (AoE[j] == 1)
            {
                try
                {
                    switch (j)
                    {
                        case 0:
                            aoeTiles.Add(tiles[x - 1, y + 1]);
                            break;
                        case 1:
                            aoeTiles.Add(tiles[x, y + 1]);
                            break;
                        case 2:
                            aoeTiles.Add(tiles[x + 1, y + 1]);
                            break;
                        case 3:
                            aoeTiles.Add(tiles[x - 1, y]);
                            break;
                        case 4:
                            break;
                        case 5:
                            aoeTiles.Add(tiles[x + 1, y]);
                            break;
                        case 6:
                            aoeTiles.Add(tiles[x - 1, y - 1]);
                            break;
                        case 7:
                            aoeTiles.Add(tiles[x, y - 1]);
                            break;
                        case 8:
                            aoeTiles.Add(tiles[x + 1, y - 1]);
                            break;
                        default:
                            break;

                    }
                }
                catch (IndexOutOfRangeException) { }
                
            }
        }


        foreach (Tile tile in aoeTiles)
        {
            tile.setMat(aoeMat);

        }
    }

    public ArrayList getNeighbors(Tile t)
    {
        ArrayList neighbors = new ArrayList();

        int x = t.getX();
        int y = t.getY();

        try {
            neighbors.Add(tiles[x - 1, y + 1]);
        }
        catch (IndexOutOfRangeException) { }
        try {
            neighbors.Add(tiles[x, y + 1]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x + 1, y + 1]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x - 1, y]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x + 1, y]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x - 1, y - 1]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x, y - 1]);
        }
        catch (IndexOutOfRangeException) { }
        try { 
        neighbors.Add(tiles[x + 1, y - 1]);
        }
        catch (IndexOutOfRangeException) { }

        return neighbors;
    }

}
