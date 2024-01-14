using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//script to power the task logic. done to store the information about how many tasks there are and how many have been done
//also visually represent them using TMP UI

//currently, the only available task is to locate positions
public class TaskManager : MonoBehaviour
{

    public Transform player;
    //this list consists of position objectives, whose information is get from the DataHolder. 
    private List<Vector3> posObj;
    //simplified position objectives, used to visually represent the pos objectives on the game map
    private List<Vector3Int> simplifiedPosObj;
    private int totalPosObjectives;
    private int finishedPosObjectives = 0;
    public TextMeshProUGUI text;
    public GameObject finishedText;
    public MapContainer mapContainer;
    private bool active = false;
    private bool finished = false;
    private MissionSoundManager missionSoundManager;

    void Awake(){
        missionSoundManager = GameObject.Find("Mission Sound Manager").GetComponent<MissionSoundManager>();
    }
    void Start()
    {
        //getting the amount of existing tasks
        posObj = DataHolder.posObj;
        simplifiedPosObj = DataHolder.simplifiedPosObj;
        totalPosObjectives = posObj.Count;

        //if there is no task, do not show the UI
        if (totalPosObjectives != 0){
            active = true;
        }

        if (active){
            ReflectCurrentState();
        }

        else{
            text.text = "";
        }
    }
    void Update()
    {   
        //for each frame, will check whether the player position is within a certain range / radius from the position objective. if so, will considered the task done and remove
        //the visual representation of the objective on the game map. also, incrementing the total task that has been done and invoking mission completion sound
        if (active){
            List<Vector3> posObjCopy = new List<Vector3>(posObj);
            List<Vector3Int> simplifiedPosObjCopy = new List<Vector3Int>(simplifiedPosObj);
            for (int i = 0; i < posObjCopy.Count; i++){
                
                Vector3 position = posObjCopy[i];
                Vector3Int simplifiedPos = simplifiedPosObjCopy[i];

                Vector3 distance = position - player.position;
                float distanceLength = FunctionKit.GetVector3Length(distance);
                if (distanceLength < 6){

                    missionSoundManager.enabled = true;
                    missionSoundManager.PlaySound();
                    finishedPosObjectives += 1;
                    posObj.Remove(position);
                    simplifiedPosObj.Remove(simplifiedPos);
                    int layerIdx = simplifiedPos.y;
                    GameLayerManager gameLayerManager = mapContainer.layers[layerIdx].GetComponent<GameLayerManager>();
                    gameLayerManager.cells[simplifiedPos.x, simplifiedPos.z].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }   
            }
            ReflectCurrentState();
        }
    }
    void ReflectCurrentState(){
        if (!finished){
            text.text = $"Position located: {finishedPosObjectives}/{totalPosObjectives}";
            //if all task has been done, temporarily show the completion title for 5 seconds
            if (finishedPosObjectives == totalPosObjectives){ 
                finishedText.SetActive(true);
                Invoke("DisableFinishText", 5.0f);            
                finished = true;
            }
        }

    }
    void DisableFinishText(){
        finishedText.SetActive(false);
    }
}
