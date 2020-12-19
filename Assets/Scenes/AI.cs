using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AI : MonoBehaviour
{
    public GameBoard board;
    public Transform smilzo;
    public Transform Cizzilo;

    public Tile tile = null;
    // Start is called before the first frame update
    public int numberOfKills = 0;
   
    public void makeMove(Tile[,] tiles)
    {

        tile = findEmpty(tiles);
        if (tile != null)
        {
            int k = UnityEngine.Random.Range(0, 2);

            if (k == 0) 
            { 
                board.spawnUnit(smilzo); 
            }
            else
            {
                board.spawnUnit(Cizzilo);
            }
        }
    }
    public void Attack()
    {

        ArrayList pieces = board.enemies;
        int range = UnityEngine.Random.Range(0, pieces.Count - 1);
        Tile piece = (Tile)pieces[range];

        ArrayList tiles = tileToAoeTiles(piece);

        foreach (Tile tile in tiles)
        {
            if (tile.getUnit() != null)
            {
                Destroy(tile.getUnit().gameObject);

                if (board.enemies.Contains(tile))
                {
                    board.enemies.Remove(tile);
                    numberOfKills += 1;
                }

                if (board.friendlies.Contains(tile))
                {
                    board.enemies.Remove(tile);
                    numberOfKills -= 1;
                }

                tile.setUnit(null);
            }
        }

        board.turn = true;

    }

    public void movePiece()
    {
        ArrayList pieces = board.enemies;

        bool pieceNotPlaced = true;
        int counter = pieces.Count * 8;

        while (pieceNotPlaced)
        {
            if (counter <= 0)
            {
                break;
            }
            int range = UnityEngine.Random.Range(0, pieces.Count - 1);
            Tile piece = (Tile)pieces[range];
            ArrayList neighbors = board.getNeighbors(piece);
            Tile spot = (Tile)neighbors[UnityEngine.Random.Range(0, neighbors.Count - 1)];
            
            if (spot.getUnit() == null)
            { 
                board.enemies.Remove(piece);
                Destroy(piece.getUnit().gameObject);
                spot.setUnit(piece.getUnit());
                tile = spot;
                board.spawnUnit(spot.getUnit().prefab);
                piece.setUnit(null);
                pieceNotPlaced = false;
                break;
            }
            counter--;
        }

        board.turn = true;

    }

    private Tile findEmpty(Tile[,] tiles)
    {
        ArrayList possibleMoves = new ArrayList();

        for(int x = 0; x < board.rows; x++)
        {
            for (int y = 0; y < board.cols; y++)
            {
                if (tiles[x,y].getUnit() == null)
                {
                    possibleMoves.Add(tiles[x, y]);
                }
            }
        }

        if (possibleMoves.Count == 0)
        {
            return null; 
        }

        else
        {
            int k = UnityEngine.Random.Range(0, possibleMoves.Count - 1);
            return (Tile)possibleMoves[k];
        }
    }

    private ArrayList tileToAoeTiles(Tile piece)
    {
        ArrayList aoeTiles = new ArrayList();
        Tile[,] tiles = board.tiles;

        int x = piece.getX();
        int y = piece.getY();
        print(x + " " + y);
        int[] AoE = piece.getUnit().getAoE();

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

        return aoeTiles;
    }
}
