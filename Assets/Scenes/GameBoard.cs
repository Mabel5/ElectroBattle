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
    TurnOptions turnType;

    public ArrayList friendlies;
    public ArrayList enemies;

    public ArrayList aoeTiles;
    public Tile currentSelected;
    public AI ai;

    public int rows = 5;
    public int cols = 5;

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
                    tile.setUnit(null);
                }
            }
        }

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
        mouseToBoard();


        if (turnType != TurnOptions.None)
        {
            switch (turnType) {
                case TurnOptions.Place:
                    mouseToSelect();
                    break;
                case TurnOptions.Attack:
                    break;
                case TurnOptions.Move:
                    break;
                default:
                    break;
            }
        } 

        if (turn != true) {
            turnType = TurnOptions.None;
            ai.makeMove(tiles);
        }
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
            friendlies.Add(unit);
            turn = !turn;
        }

        else if (turn == false)
        {
            Transform unit = Instantiate(unitPrefab, new Vector3(ai.tile.getX(), unitPrefab.position.y, ai.tile.getY()), Quaternion.identity);

            Vector3 rot = unit.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);
            unit.rotation = Quaternion.Euler(rot);
            ai.tile.setUnit(unit.GetComponent<Unit>());
            enemies.Add(unit);
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

    public void Move() { 

    }

    public void Attack(Unit unit)
    {

    }

    public void Place()
    {

    }
}
