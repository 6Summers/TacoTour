using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private bool paused = false;

    [SerializeField] private float defaultVelocity = 5;
    private float currentVelocity;
    
    
    [FormerlySerializedAs("pawseCanvas")] [SerializeField] private GameObject pauseScreen;

    [SerializeField] private PlayerMovement playerController;
    
    [SerializeField] private GameObject background;
    [SerializeField] private Transform level;
    
    private BackgroundLoop backgroundMovement; 
    private CameraMovement cameraMovement; 
    
    // Start is called before the first frame update
    void Start()
    {
        currentVelocity = defaultVelocity;
        playerController.setCurrentVelocity(defaultVelocity);
        
        backgroundMovement = background.GetComponent<BackgroundLoop>();
        cameraMovement = background.GetComponent<CameraMovement>();
        
        playerController.Pause(paused);
        backgroundMovement.setPaused(paused);
        
        pauseScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
           PauseGame();
        }
    }

    private void PauseGame()
    {
        paused = !paused;
        Debug.Log("pausado " + paused);

        pauseScreen.SetActive(paused);
            
        backgroundMovement.setPaused(paused);
        playerController.Pause(paused);
        cameraMovement.Pause(paused);

        if (paused)
            playerController.getAnimator().speed = 0;

        else
            playerController.getAnimator().speed = 1;

    }

    public void PauseCamera(bool pause)
    {
        cameraMovement.Pause(pause);
    }
    public void ChangePlatformVelocity(float newVelocity)
    {
        int platformAmount = level.childCount;
        PlatformMovement platform;
        
        //Goes through all the children of the game object containing the design of the level and changes their speed
        for (int i = 0; i < platformAmount; i++)
        {
            platform = level.GetChild(i).GetComponent<PlatformMovement>();
            if (!platform.IsUnityNull())
            {
                platform.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
    }
    public float getDefaultVelocity()
    {
        return defaultVelocity;
    }


}
