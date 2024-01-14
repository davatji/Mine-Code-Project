using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//player game object: essentially empty game object with child game object -> camera
public class PlayerScript : MonoBehaviour
{
    public GameObject torch;
    public Transform playerArrow;
    public float mouseSensitivity = 1;
    private Transform playerCamera;
    private World world;
    private float playerRadius = 0.3f;
    private float playerHeight = 2f;
    private const float gravitationalAcceleration = -9.8f;
    private const float terminalVelocity = -20f;
    private float sneakSpeed = 1.5f;
    private float marchingSpeed = 3;
    private float sprintingSpeed = 6;
    private float jumpForce = 5; 

    //initializing variables necessary for holding player input information in normalized axis
    private float horizontal;
    private float vertical;
    private float mouseX;
    private float mouseY;
    private float movementState;
    private float movementSpeed;
    private bool jumpBuffer;
    private bool toggleTorchBuffer = false;
    private bool useTorch = true;

    private Vector3 velocityVector;
    private Vector3 movementVector;
    private float verticalVelocity;

    private bool isOnGround;

    void Start()
    {
        playerCamera = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        //initial vertical velocity = 0
        verticalVelocity = 0;
        jumpBuffer = false;
        isOnGround = true;
        torch.SetActive(useTorch);
    }

    //separate update func for capturing input: making sure along all frame intervals, the player input will be captured and no miss for jump input

     /* ===FOR EACH FRAME===
        1. capturing the input
        2. the captured input will be used to determine how much is the player movement within this frame (in Vector3)
        3. the captured input will also be used to rotate the camera or the player
        4. the captured input is used to toggle the torch state or jump
    */

    void Update(){
        CaptureInput();
    }
    void FixedUpdate()
    {
        float normalizedPlayerPosX = playerCamera.transform.position.x / DataHolder.worldLengthInVoxel;
        float normalizedPlayerPosZ = playerCamera.transform.position.z / DataHolder.worldWidthInVoxel;

        float adjustedPlayerPosX = normalizedPlayerPosX * 450 - 225;
        float adjustedPlayerPosZ = normalizedPlayerPosZ * 300 - 150;

        if (toggleTorchBuffer){
            useTorch = !useTorch;
            torch.SetActive(useTorch);
            toggleTorchBuffer = false;
        }   

        playerArrow.localPosition = new Vector3(adjustedPlayerPosX, adjustedPlayerPosZ, 0);
        playerArrow.Rotate(new Vector3(0, 0, -mouseX * mouseSensitivity));

        //sneak mode, visually positionate down the camera relative to the player position
        if (movementState < 0 && playerCamera.transform.localPosition.y == 1.75f){
            playerCamera.transform.localPosition = new Vector3(0, 1.5f, 0);
        }
        else if (movementState >= 0 && playerCamera.transform.localPosition.y == 1.5f){
            playerCamera.transform.localPosition = new Vector3(0, 1.75f, 0);
        }

        if (jumpBuffer){
            Jump();
            jumpBuffer = false;
        }

        transform.Rotate(new Vector3(0, mouseX * mouseSensitivity, 0));
        playerCamera.Rotate(new Vector3(-mouseY * mouseSensitivity, 0, 0));

        CalculateMovement();
    
        transform.position += movementVector;
    }

    void CalculateMovement(){
        //for each frame, will increment the vertical movement by gravitational acceleration adjusted 
        //--> implying given a full second of fall, the vertical movement would be -9.8 at the end of that second

        //pseudo terminal velocity, where vertical velocity would eventually reach a constant due to the presence of air resistance
        if (verticalVelocity > terminalVelocity){
            verticalVelocity += gravitationalAcceleration * Time.fixedDeltaTime;
        }
        
        //movement speed will vary according to whether shift/ctrl is being pressed
        if (movementState > 0){
            movementSpeed = sprintingSpeed;
        }
        else if (movementState < 0){
            movementSpeed = sneakSpeed;
        }
        else{
            movementSpeed = marchingSpeed;
        }
        //deltatime to adjust the movement from per frame to per second
        velocityVector = (new Vector3(0, 0, 1) * vertical + new Vector3(1, 0, 0) * horizontal) * movementSpeed;
        movementVector = velocityVector * Time.fixedDeltaTime;
        movementVector += new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime;

        //adjust the player movement from local to global vector (depending on the player and camera rotation)
        movementVector = transform.TransformDirection(movementVector);
        
        //if the projected global position after movement is occupied by a solid block, cancel the movement
        //incorporating player radius so that the pseudo-collision happens with player's outer border
        if (world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(playerRadius, 0, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(-playerRadius, 0, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(playerRadius, 0, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(-playerRadius, 0, -playerRadius)) ||

            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(playerRadius, 1, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(-playerRadius, 1, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(playerRadius, 1, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x, 0, 0) + new Vector3(-playerRadius, 1, -playerRadius))){
            movementVector.x = 0;
        }
        if (world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(playerRadius, 0, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(-playerRadius, 0, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(playerRadius, 0, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(-playerRadius, 0, -playerRadius)) ||
            
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(playerRadius, 1, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(-playerRadius, 1, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(playerRadius, 1, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(0, 0, movementVector.z) + new Vector3(-playerRadius, 1, -playerRadius))){
            movementVector.z = 0;
        }

        //if one of the blocks below player is solid, cancel the calculated vertical movement and reset the accumulated value to zero
        if (world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, movementVector.y, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, movementVector.y, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, movementVector.y, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, movementVector.y, -playerRadius))){
            movementVector.y = 0;
            verticalVelocity = 0;
            isOnGround = true;
        }        

        if (movementVector.y > 0 && 
            (world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, movementVector.y + playerHeight, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, movementVector.y + playerHeight, playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, movementVector.y + playerHeight, -playerRadius)) ||
            world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, movementVector.y + playerHeight, -playerRadius)))){
            movementVector.y = 0;
            verticalVelocity = 0;
        }        

        //in sneak mode, when projected x or z movement would result in the whole player body entering a voxel territory whose underneath voxel is not solid: cancel that movement
        if (movementState < 0){
            if (movementVector.x != 0 && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x - playerRadius, -1, playerRadius)) && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x + playerRadius, -1, playerRadius)) &&
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x - playerRadius, -1, -playerRadius)) && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(movementVector.x + playerRadius, -1, -playerRadius))){
                movementVector.x = 0;
                }
            if (movementVector.z != 0 && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, -1, movementVector.z - playerRadius)) && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(playerRadius, -1, movementVector.z + playerRadius)) &&
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, -1, movementVector.z - playerRadius)) && 
                !world.IsSolidVoxelGlobal(transform.position + new Vector3(-playerRadius, -1, movementVector.z + playerRadius))){
                movementVector.z = 0;
                }
        }
    }
    void CaptureInput(){
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        movementState = Input.GetAxis("Movement State");       
        if (Input.GetAxis("Jump") > 0 && isOnGround){
            //signaling the FixedUpdateFunction so that for the next fixed update frame, jump will be performed
            //the buffer is used to ensure all jump attempt, when certain conditions are met, will always be accomodated and not missed

            jumpBuffer = true;
        }
        if (Input.GetKeyDown("t")){
            toggleTorchBuffer = true;
        }
    }
    void Jump(){
        verticalVelocity += jumpForce;
        isOnGround = false;
    }

    //pause the game by setting the time scale to 0 (effectively removing the effect of every function that relies on Time.deltaTime but not Time.unscaledDeltaTime)
    public void TogglePlayerTimeScale(bool pause){
        if (pause){
            Time.timeScale = 0;
        }
        else{
            Time.timeScale = 1;
        }
    }
}
