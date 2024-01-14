using UnityEngine;

public class GameLayerManager : MonoBehaviour
{
    //layer map that contains public information about the cells gameobject. during the instantiation through MapContainer class, will iterate through the each cells to modify the color 
    //according to the simplified map array (so that the map visually actually represents the simplified map)
    public GameObject[,] cells;
    void Awake(){
        cells = new GameObject[DataHolder.mapBluePrint.GetLength(0), DataHolder.mapBluePrint.GetLength(2)];
    }
}