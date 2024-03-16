using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class PlayerMovement : MonoBehaviour
{
    private PlatformMovement platformMovement;
    private CameraMovement cameraMovement;
    private BackgroundLoop backgroundMovement;
    
    public float speed;
    private float jumpForce = 50f;
    private Rigidbody2D rigidbody;
    private Animator animator;
    private bool catToDog;
    
    //enum for checking character state
    private enum PlayerState {Running,  Crouching, Reaching, Clinging, Falling, Stunned}
    private PlayerState playerState = PlayerState.Running;
    
    [Header("Character Change Variables")]
    [SerializeField] private float changeCooldown =1.2f;
    private float reSpawnTourTime = 2f;
    private bool delayCat = false;
    private float cooldown;
    private float defaultVelocity = 0f;
    private bool powerUpActivated = false;
    private bool paused = false;
    [SerializeField] private bool isTaco = true;

    [Header("Jumps")] 
    [SerializeField] private float catJump;
    [SerializeField] private float dogJump;
    private bool isActionActive = false;                        //This variable is to know if the dog is dizzy
    [SerializeField] private float dizzyDog = 1;               //Dizy time 
    private float actionTimer = 0f;                             //Timer to track the elapsed time of the action
    private int ceilingCollisionCount = 0;                      //to control when the character is collisioning with the ceiling
    
    private Button optionButton;
    private Button closeButton;
    private TextMeshProUGUI pawseCanvas;
    private TextMeshProUGUI pressP;
    
   [SerializeField] private GameObject background; 
   
    private float powerUpTimer = 0f;                            //Timer to control how long the power up is active 
    private float limitTimePowerUp = 5.0f;
    private float defaultCameraVelocity;
    private bool invulnerableState = false;

    [Header("Power-Ups UI")]
    [SerializeField] private Transform powerUpUIParent; 
    [SerializeField] private GameObject powerUpUIPrefab;
    private List<GameObject> powerUpItems = new List<GameObject>(); // Tracks the current power-up icons
    private Slider progressBar;
    private GameObject barObject = null;
    void Start()
    {
        GameObject platform = GameObject.FindWithTag("Platform");
        pawseCanvas = GameObject.Find("Pawse").GetComponent<TextMeshProUGUI>();
        pressP = GameObject.Find("PressP").GetComponent<TextMeshProUGUI>();
        optionButton = GameObject.Find("OptionButton").GetComponent<Button>();
        closeButton = GameObject.Find("CloseButton").GetComponent<Button>();
        platformMovement = platform.GetComponent<PlatformMovement>();
        barObject = GameObject.Find("BarObject");
        
        if (barObject != null)
        {
            progressBar = barObject.GetComponentInChildren<Slider>();
            barObject.SetActive(false);
        }
        
        cameraMovement = background.GetComponent<CameraMovement>();
        backgroundMovement = background.GetComponent<BackgroundLoop>();
        defaultCameraVelocity = cameraMovement.GetCameraSpeed();
        
        defaultVelocity = platformMovement.GetSpeed();
        rigidbody =  gameObject.GetComponent<Rigidbody2D>(); 
        animator = gameObject.GetComponent<Animator>();
        
        animator.SetBool("isTaco", isTaco);
        jumpForce = dogJump;
        pawseCanvas.enabled = false;
        pressP.enabled = false;
        optionButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            paused = !paused;
            Debug.Log("pausado " + paused);
            
            if (paused)
            {
                applyVelocity(0);
                applyVelocityCamera(0);
                pawseCanvas.enabled = true;
                optionButton.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(true);
                animator.speed = 0;
                backgroundMovement.setIsPaused(true);
                pressP.enabled = true;
            }
            else
            {
                applyVelocity(defaultVelocity);
                applyVelocityCamera(defaultCameraVelocity);
                pawseCanvas.enabled = false;
                pressP.enabled = false;
                optionButton.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(false);
                animator.speed = 1;
                backgroundMovement.setIsPaused(false);
            }
        }

        if (!paused)
        {
            if(cooldown>0) cooldown--;
        
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !animator.GetBool("crouching"))
            {
                Jump();
            }
            if (Input.GetKeyDown(KeyCode.S) && IsGrounded())
            {
                animator.SetBool("crouching", true);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                animator.SetBool("crouching", false);
            }

            if (Input.GetKeyDown((KeyCode.K)) && cooldown <= 0) //add cooldown check
            {
                isTaco = !isTaco;
                bool theAnimator = animator.GetBool("crouching");
                if (theAnimator)
                {
                    Debug.Log("animator true");
                    animator.SetBool("crouching", false);
                }
                animator.SetBool("isTaco", isTaco);

                if (!isTaco)
                {
                    catToDog = true;
                    jumpForce = catJump;
                }
                else jumpForce = dogJump;
                
                cooldown = changeCooldown;
                
            }
            
            if (Input.GetKeyDown(KeyCode.A) && !isTaco && playerState != PlayerState.Clinging && !IsGrounded())
            {
                playerState = PlayerState.Reaching;
                animator.SetTrigger("isReaching");

            }
            
            if ((Input.GetKeyUp(KeyCode.A) || isTaco) && playerState == PlayerState.Clinging) //if the player stops clinging or if they change to taco
            {
                rigidbody.gravityScale = 1.3f;
                playerState = PlayerState.Falling;
                animator.SetTrigger("isFalling");

            }

            if (isActionActive)
            {
                actionTimer += Time.deltaTime;                          //We increment timer

                //if the timer reaches the duration, we deactivate the action .
                if (actionTimer >= dizzyDog)
                {
                    isActionActive = false;
                    actionTimer = 0f;                                   //We reset the timer 
                }
            }
            
            if (playerState == PlayerState.Falling && isTaco && IsGrounded() && !isActionActive)
            {
                isActionActive = true;
                rigidbody.velocity = new Vector2(-dizzyDog, rigidbody.velocity.y); //We make taco "go to the left"
                playerState = PlayerState.Running; //We reset the player state to running
            }

            if (Input.GetKeyDown(KeyCode.L) && !powerUpActivated && powerUpItems.Count > 0)
            {
                powerUpActivated = true;
                barObject.SetActive(true);
                int lastIndex = powerUpItems.Count - 1;
                GameObject iconToRemove = powerUpItems[lastIndex];
                powerUpItems.RemoveAt(lastIndex);
                Destroy(iconToRemove);
                if (isTaco)
                {
                    applyVelocity(defaultVelocity * 1.2f);
                    progressBar.maxValue = limitTimePowerUp;
                    progressBar.value = limitTimePowerUp;
                    powerUpTimer = limitTimePowerUp;
                    Debug.Log("progress bar val " + progressBar.value + " " + limitTimePowerUp );
                    //change the animation of the dog
                }
                else
                {
                    progressBar.maxValue = limitTimePowerUp*4;
                    progressBar.value = limitTimePowerUp*4;
                    powerUpTimer = limitTimePowerUp*4;
                    invulnerableState = true;
                    
                    
                    //change the animation of the cat
                }
                
            }
            //Show when the power up is activated
            if (powerUpActivated) //if (powerUpActivated && powerUpImage.enabled)
            {
                if (isTaco)
                {
                    if (catToDog)
                    {
                        powerUpTimer = 0; 
                        applyVelocity(defaultVelocity);
                        powerUpActivated = false;
                        invulnerableState = false;
                        barObject.SetActive(false);
                    }
                    if (powerUpTimer <= 0)
                    {
                        powerUpTimer = 0;
                        applyVelocity(defaultVelocity);
                        powerUpActivated = false;
                        barObject.SetActive(false);
                    }
                    else
                    {
                        powerUpTimer -= Time.deltaTime;
                        progressBar.value -= Time.deltaTime;
                  
                        if(platformMovement.GetSpeed() < defaultVelocity * 1.5f)
                            applyVelocity(defaultVelocity * 1.5f);
                    }
                }
                else //if is Tour
                {
                    if (platformMovement.GetSpeed() > defaultVelocity)
                    {
                        powerUpTimer = 0; 
                        applyVelocity(defaultVelocity);
                        powerUpActivated = false;
                        invulnerableState = false;
                        barObject.SetActive(false);
                    }

                    if (powerUpTimer <= 0)
                    {
                        powerUpTimer = 0;
                        powerUpActivated = false;
                        invulnerableState = false;
                        barObject.SetActive(false);
                        invulnerableState = false;
                    }
                    else
                    {
                        powerUpTimer -= Time.deltaTime;
                        progressBar.value -= Time.deltaTime;
                        invulnerableState = true;
                    }
                }
            
            }
            else if (delayCat)
            {
                if (reSpawnTourTime <= 0)
                {
                    //animator.SetBool("tour_run",false);
                    //animator.SetBool("isTaco", false);
                    //animator.SetBool("crouching", false);
                    applyVelocity(defaultVelocity);
                    applyVelocityCamera(defaultCameraVelocity);
                    reSpawnTourTime = 2f;
                }
                else
                {
                    reSpawnTourTime -= Time.deltaTime;
                }
            }
        }//end if paused
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            if(!invulnerableState)
                KillPlayer();
            else // compara si y < 0
            {
                //pausar juego
                //lift Tour on a platform
                applyVelocity(0);
                applyVelocityCamera(0);
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, 5f);
                animator.SetTrigger("isReaching"); //change animation to revive
                powerUpTimer = 0;
                delayCat = true;
                SpawnPlatformAtPosition();
            }
        }
        else if (other.CompareTag("Ceiling")) 
        {
            ceilingCollisionCount++;                                                // Increment the counter
            
            if (playerState == PlayerState.Reaching || playerState == PlayerState.Clinging)
            {
                GameObject ceiling = other.GameObject();
                Vector2 ceilingPosition = ceiling.transform.position;
                Vector2 characterPosition;

                characterPosition.x = transform.position.x;
                characterPosition.y = ceilingPosition.y;
                characterPosition.y += ceiling.GetComponent<SpriteRenderer>().bounds.size.y/ 2 -
                                       GetComponent<SpriteRenderer>().bounds.size.y / 2;
            
                //stop gravity simulation
                rigidbody.velocity  = new Vector2(0, 0);
                //rigidbody.bodyType = RigidbodyType2D.Kinematic;
                rigidbody.gravityScale = 0f;

                transform.position = characterPosition;

                playerState = PlayerState.Clinging;
            }
            
        }
        
        if (other.CompareTag("PowerUp")) 
        {
            Destroy(other.gameObject);

            // Check if we can add more power-up icons
            if (powerUpItems.Count < 3)
            {
                GameObject newIcon = Instantiate(powerUpUIPrefab, powerUpUIParent);
                
                // Calculate the new position
                
                float offsetX = powerUpItems.Count * 30;
                
                newIcon.transform.localPosition = new Vector3(offsetX , 0, 0); 
                powerUpItems.Add(newIcon);

            }
            
        }
        
        if (other.CompareTag("EndZone"))
        {
            Destroy(this.GameObject());
            SceneManager.LoadScene("EndTutorial");
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ceiling"))
        {
            ceilingCollisionCount--; // Decrement the counter

            if (ceilingCollisionCount <= 0 && playerState == PlayerState.Clinging)
            {
                rigidbody.gravityScale = 1.3f;
                playerState = PlayerState.Falling;
                animator.SetTrigger("isFalling");
            }

            if (playerState == PlayerState.Reaching)
            {
                rigidbody.gravityScale = 1.3f;
                playerState = PlayerState.Falling;
                animator.SetTrigger("isFalling");
            }
        }
    }


    private void OnCollisionStay2D(Collision2D other)
    {
        //if (other.gameObject.tag == "Platform")
        if (other.gameObject.tag == "CrouchZone")
        {
            transform.position = new Vector3(transform.position.x, -4.107f, transform.position.z);
        
            Vector3 position = gameObject.GetComponent<Transform>().position;
            position = new Vector3(position.x - speed * Time.deltaTime, position.y, position.z);
            gameObject.GetComponent<Transform>().position = position;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Platform" && delayCat)
        {
            animator.SetTrigger("isFalling");
            animator.SetBool("isTaco", isTaco);
        }
    }

    private void SpawnPlatformAtPosition()
    {
        float x = 0;
        Vector3 position = new Vector3(x, transform.position.y, transform.position.z);
        GameObject platformPrefab = Resources.Load<GameObject>("platformSmall");
        if (platformPrefab != null)
        {
            // Instantiate the prefab at the specified position and default rotation
            Instantiate(platformPrefab, position, Quaternion.identity);
        }
    }
    bool IsGrounded()
    {
        return rigidbody.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    void Jump()
    {
        
        rigidbody.velocity = new Vector2(0, jumpForce);
        
    }

    private void KillPlayer()
    {
        Destroy(this.GameObject());
        SceneManager.LoadScene("GameOver");
    }

    private void applyVelocityCamera(float newVelocity)
    {
        cameraMovement = background.GetComponent<CameraMovement>();
        
        if(cameraMovement != null)
            cameraMovement.GetComponent<CameraMovement>().SetCameraSpeed(newVelocity);
    }

    private void applyVelocity(float newVelocity)
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platforms)
        {
            PlatformMovement platformMovement = platform.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                platformMovement.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
            
        GameObject[] ceilings = GameObject.FindGameObjectsWithTag("Ceiling");
        foreach (GameObject ceiling in ceilings)
        {
            PlatformMovement platformMovement = ceiling.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                platformMovement.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
            
        GameObject[] crouchZones = GameObject.FindGameObjectsWithTag("Floor");
        foreach (GameObject crouchZone in crouchZones)
        {
            PlatformMovement platformMovement = crouchZone.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                platformMovement.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
        GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
        foreach (GameObject powerUp in powerUps)
        {
            PlatformMovement platformMovement = powerUp.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                platformMovement.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
        GameObject[] endzone = GameObject.FindGameObjectsWithTag("EndZone");
        foreach (GameObject endZones in endzone)
        {
            PlatformMovement platformMovement = endZones.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                platformMovement.SetSpeed(newVelocity); // Change the speed to the desired value
            }
        }
        
    }

}
