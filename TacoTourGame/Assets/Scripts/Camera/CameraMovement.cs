using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float defaultCameraSpeed = -2f;
    private float cameraSpeed;
    private bool isPaused = false;
    
    //we get the rect transform
    private RectTransform rectTransform;
    
    void Start()
    {
       cameraSpeed = defaultCameraSpeed;
       rectTransform = GetComponent<RectTransform>();
    }
    // Update is called once per frame
    void Update()
    {

        if (!isPaused)
        {
            float newPosition = rectTransform.anchoredPosition.x + cameraSpeed * Time.deltaTime;
            //We apply the movement to the camera 
            rectTransform.anchoredPosition = new Vector2(newPosition, rectTransform.anchoredPosition.y);
        }

    }

    public void Pause(bool pause)
    {
        isPaused = pause;
        
        if (isPaused)
            cameraSpeed = 0f;
        else
            cameraSpeed = defaultCameraSpeed;
        
    }
    
}