using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform prefab;
    public int[] getAoE()
    {
        if(this.gameObject.tag == "Smilzo")
        {
            return new int[] {0, 1, 0,
                              1, 0, 1,
                              0, 1, 0}; 
        }
        if(this.gameObject.tag == "Cizzio")
        {
            return new int[] {1, 0, 1,
                              0, 0, 0,
                              1, 0, 1};
        }
        return null;
    }

}

