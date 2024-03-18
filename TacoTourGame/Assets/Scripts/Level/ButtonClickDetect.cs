using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ButtonClickDetect: MonoBehaviour
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

    // Method to handle ButtonPlay click event
    void HandlePlayButtonClick()
    {
        //SceneManager.LoadScene("SampleScene");
        //changed for tutorial testing
        SceneManager.LoadScene("TutorialDesign");
        
    }

    // Method to handle ButtonClose click event
    void HandleCloseButtonClick()
    {
        Application.Quit();
    }
}