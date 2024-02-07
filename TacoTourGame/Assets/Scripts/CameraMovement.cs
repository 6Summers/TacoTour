using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{ 
    [SerializeField] private float cameraSpeed = 1.0f;
    
    //we get the rect transform
    private RectTransform rectTransform;
    
    void Start()
    {
       rectTransform = GetComponent<RectTransform>();
    }
    // Update is called once per frame
    void Update()
    {
        

        float newPosition = rectTransform.anchoredPosition.x + cameraSpeed * Time.deltaTime;
        //We apply the movement to the camera 
        rectTransform.anchoredPosition = new Vector2(newPosition, rectTransform.anchoredPosition.y);
    }
}