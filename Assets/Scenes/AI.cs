using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public GameBoard board;
    public Transform smilzo;
    public Transform Cizzilo;

    public Tile tile = null;
    // Start is called before the first frame update

    public void makeMove(Tile[,] tiles)
    {

        tile = findEmpty(tiles);
        if (tile != null)
        {
            int k = Random.Range(0, 2);

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
            int k = Random.Range(0, possibleMoves.Count - 1);
            return (Tile)possibleMoves[k];
        }
    }
}
