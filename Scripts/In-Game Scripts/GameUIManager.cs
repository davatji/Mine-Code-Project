using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;


//class responsible for general UI logic
public class GameUIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool pause = false;
    private bool requestUpdatePauseState = false;

    public GameObject debugScreen;
    public Text FPSText;
    public Text coorText;

    public GameObject buttonUp;
    public GameObject buttonDown;
    public GameObject mapContainer;
    public GameObject currLayerIndicator;
    public Image arrow;

    private GameLayerButton upButtonScript;
    private GameLayerButton downButtonScript;

    public GameObject player;
    private PlayerScript playerScript;
    private World world;
    
    private bool showDebugScreen = false;
    private bool requestUpdateDebugState = false;
    private int countFrame = 0;
    private float accumulatedTime = 0;
    private bool requestIncrementMapLayer = false;
    private bool requestDecrementMapLayer = false;
    private bool requestToggleShowMap = false;
    private bool showMap = true;

    private Color32 visibleColor = new Color32(255, 255, 255, 255);
    private Color32 invisibleColor = new Color32(255, 255, 255, 0);

    //initializing variable value and its visual representation
    void Start(){
        world = GameObject.Find("World").GetComponent<World>();
        playerScript = player.GetComponent<PlayerScript>(); 

        upButtonScript = buttonUp.GetComponent<GameLayerButton>();
        downButtonScript = buttonDown.GetComponent<GameLayerButton>();
        
        UpdatePauseState();
        UpdateDebugState();

        FPSText.text = $"FPS: -";
        coorText.text = $"X: -\nY: -\nZ: -";
    }

    /*for each frame, capture the player input and according to that input, send a buffer signal so that in the next frame, a further action would be elaborated:
    which is to:
    1. enable/disable debugscreen, which if activated, the next each frame would update the value of debug screen text (FPS and coordinate value) by repeatedly referring
       to the player absolute position and Time.unscaledDeltaTime value in world script
    2. pause the game, which is essentially setting the world Time.deltaTime to zero to stop all the physical mechanism and showing the pause UI that allows user to change scene
    3. increment or decrement the map layer by pressing up or down arrow button
    4. toggle torch
    5. toggle map 
    */
    void Update(){
        if (requestUpdateDebugState){
            ToggleDebug();
            requestUpdateDebugState = false;
        }
        if (showDebugScreen){
            UpdateDebugInfo();
        }
        if (requestUpdatePauseState){
            TogglePause();
            requestUpdatePauseState = false;
        }
        if (requestIncrementMapLayer){
            upButtonScript.Increment();
            requestIncrementMapLayer = false;
        }
        if (requestDecrementMapLayer){
            downButtonScript.Decrement();
            requestDecrementMapLayer = false;
        }
        if (requestToggleShowMap){
            ToggleShowMap();
            requestToggleShowMap = false;
        }
        CaptureInput();
    }
    void CaptureInput(){
        if (Input.GetKeyDown("tab")){
            requestUpdateDebugState = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            requestUpdatePauseState = true;
        }
        if (Input.GetKeyDown("up")){
            requestIncrementMapLayer = true;
        }
        if (Input.GetKeyDown("down")){
            requestDecrementMapLayer = true;
        }
        if (Input.GetKeyDown("m")){
            requestToggleShowMap = true;
        }
    }
    public void TogglePause(){
        pause = !pause;
        UpdatePauseState();
    }
    void UpdatePauseState(){
        pauseMenu.SetActive(pause);
        world.ToggleWorldTimeScale(pause);
        playerScript.TogglePlayerTimeScale(pause);
    }
    void ToggleDebug(){
        showDebugScreen = !showDebugScreen;
        UpdateDebugState();
    }
    void UpdateDebugState(){
        debugScreen.SetActive(showDebugScreen);
    }
    void UpdateDebugInfo(){
        countFrame += 1;
        //using unscaledDeltaTime so that the calculation of the FPS won't differ when being paused
        accumulatedTime += world.unscaledDeltaTime;
        if (accumulatedTime > 1){
            float averageFPS = countFrame / accumulatedTime;
            FPSText.text = $"FPS: {(int)averageFPS}";
            accumulatedTime = 0;
            countFrame = 0;
        }
        Vector3 playerPos = player.transform.position;
        coorText.text = $"X: {Math.Round(playerPos.x, 2)}\nY: {Math.Round(playerPos.y, 2)}\nZ: {Math.Round(playerPos.z, 2)}";
    }
    public void ChangeSceneFromGame(int layer){
        SceneManager.LoadScene(layer);
    }
    public void ToggleShowMap(){
        showMap = !showMap;
        mapContainer.SetActive(showMap);
        buttonUp.SetActive(showMap);
        buttonDown.SetActive(showMap);
        currLayerIndicator.SetActive(showMap);
        if (showMap){
            arrow.color = visibleColor;
        }
        else{
            arrow.color = invisibleColor;
        }
    }
}
