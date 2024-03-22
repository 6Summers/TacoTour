using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{ 
    private float cameraSpeed = -2f;
    private bool isPaused = false;
    
    //we get the rect transform
    private RectTransform rectTransform;
    
    void Start()
    {
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
    
    public float GetCameraSpeed() {
        return cameraSpeed;
    }

    public void SetCameraSpeed(float newSpeed) {
        cameraSpeed = newSpeed;
    }
    
    public bool getIsPaused()
    {
        return isPaused;
    }

    public void setIsPaused(bool option)
    {
        isPaused = option;
    }
    
}