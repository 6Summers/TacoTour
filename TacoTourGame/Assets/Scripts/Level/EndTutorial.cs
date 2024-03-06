using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

public class EndTutorial : MonoBehaviour
{
    // References to the Button components
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonClose;

    // Define UnityEvents to hold different actions for each button
    public UnityEvent actionForPlayButton;
    public UnityEvent actionForCloseButton;

    void Start()
    {
        // Check if a Button component exists for each button
        if (buttonPlay != null)
            buttonPlay.onClick.AddListener(HandlePlayButtonClick);
        else
            Debug.LogError("ButtonPlay reference not set.");

        if (buttonClose != null)
            buttonClose.onClick.AddListener(HandleCloseButtonClick);
        else
            Debug.LogError("ButtonClose reference not set.");
        
    }
    void HandlePlayButtonClick()
    {
        SceneManager.LoadScene("SampleScene"); //change to the game scene
    }

    // Method to handle ButtonClose click event
    void HandleCloseButtonClick()
    {
        Application.Quit();
    }
}
