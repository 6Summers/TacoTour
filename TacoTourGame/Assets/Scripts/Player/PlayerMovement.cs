using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    private float currentVelocity;
    private float jumpForce = 50f;
    private Rigidbody2D rigidbody;
    private Animator animator;
    
    //enum for checking character state
    private enum PlayerState {Running,  Crouching, Reaching, Clinging, Falling, Stunned}
    private PlayerState playerState = PlayerState.Running;
    
    [Header("Character Change Variables")]
    [SerializeField] private float changeCooldown =1.2f;
    private float reSpawnTourTime = 2f;
    private bool delayCat = false;
    private float cooldown;
    private bool powerUpActivated = false;
    private bool paused = false;
    [SerializeField] private bool isTaco = true;

    [Header("Jumps")] 
    [SerializeField] private float catJump;
    [SerializeField] private float dogJump;
    private bool isDizzy = false;                        //This variable is to know if the dog is dizzy
    [SerializeField] private float dizzyDog = 1;               //Dizy time 
    private float actionTimer = 0f;                             //Timer to track the elapsed time of the action
    private int ceilingCollisionCount = 0;                      //to control when the character is collisioning with the ceiling

    private float powerUpTimer = 0f;                            //Timer to control how long the power up is active 
    private float limitTimePowerUp = 5.0f;
    private bool invulnerableState = false;

    [Header("Power-Ups UI")]
    [SerializeField] private Transform powerUpUIParent; 
    [SerializeField] private GameObject powerUpUIPrefab;
    private List<GameObject> powerUpItems = new List<GameObject>(); // Tracks the current power-up icons
    private Slider progressBar;
    private GameObject barObject = null;
    
    void Start()
    {
        barObject = GameObject.Find("BarObject");
        
        if (barObject != null)
        {
            progressBar = barObject.GetComponentInChildren<Slider>();
            barObject.SetActive(false);
        }
        rigidbody =  gameObject.GetComponent<Rigidbody2D>(); 
        animator = gameObject.GetComponent<Animator>();
        
        animator.SetBool("isTaco", isTaco);
        jumpForce = dogJump;
    }

    void Update()
    {
       
        if (!paused)
        {
            if(cooldown>0) cooldown--;
        
            //Checks the different keyboard inputs
            CheckMovementInput();
            CheckCharacterChangeInput();
            CheckPowerUpInput();
            
            //If Taco is Dizzy
            if (isDizzy)
            {
                actionTimer += Time.deltaTime;                          //We increment timer

                
                //if the timer reaches the duration, we deactivate the action .
                if (actionTimer >= dizzyDog)
                {
                    isDizzy = false;
                    actionTimer = 0f;                                   //We reset the timer 
                }
            }
            
            //If it was reaching but is now touching the floor,
            //or was falling as tour and touches the floor
            //goes back to running
            if ((playerState == PlayerState.Reaching || playerState == PlayerState.Falling && !isTaco)  && IsGrounded())
            {
                playerState = PlayerState.Running;
                animator.SetTrigger("isFalling");
            }

            if (playerState == PlayerState.Falling && isTaco && IsGrounded() && !isDizzy)
            {
                isDizzy = true;
                rigidbody.velocity = new Vector2(-dizzyDog, rigidbody.velocity.y); //We make taco "go to the left"
                playerState = PlayerState.Running; //We reset the player state to running
            }

            PowerupUpdate();
            
            //Show when the power up is activated
            
        }//end if paused
        
    }
    

    private void CheckMovementInput()
    {
        //Checks for jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !animator.GetBool("crouching"))
        {
            Jump();
        }
        
        //Check to start crouching
        if (Input.GetKeyDown(KeyCode.S) && IsGrounded())
        {
            animator.SetBool("crouching", true);
        }
        
        //Check to end crouching
        if (Input.GetKeyUp(KeyCode.S))
        {
            animator.SetBool("crouching", false);
        }
        
        //Check to start reaching state    
        if (Input.GetKeyDown(KeyCode.A) && !isTaco && playerState != PlayerState.Clinging && !IsGrounded())
        {
            playerState = PlayerState.Reaching;
            animator.SetTrigger("isReaching");

        }
            
        //if the player stops clinging or if they change to taco
        if ((Input.GetKeyUp(KeyCode.A) || isTaco) && playerState == PlayerState.Clinging) 
        {
            rigidbody.gravityScale = 1.3f;
            playerState = PlayerState.Falling;
            animator.SetTrigger("isFalling");

        }
        
    }

    private void CheckCharacterChangeInput()
    {
        if (Input.GetKeyDown((KeyCode.K)) && cooldown <= 0) //add cooldown check
        {
            isTaco = !isTaco;
            bool isCrouching = animator.GetBool("crouching");
            if (isCrouching)
            {
                Debug.Log("animator true");
                    
                animator.SetBool("crouching", false);
            }
            animator.SetBool("isTaco", isTaco);

            if (!isTaco)
            {
                //changes to cat
                jumpForce = catJump;
                
                //if dash power-up is active, deactivates it
                if (powerUpTimer>0)
                {
                    powerUpTimer = 0;
                    currentVelocity = gameManager.getDefaultVelocity();
                    gameManager.ChangePlatformVelocity(currentVelocity);
                    powerUpActivated = false;
                    invulnerableState = false;
                    barObject.SetActive(false);
                }
            }
            else
            {
                //changes to dog
                jumpForce = dogJump;

                //if second life power-up is active, deactivates it
                if (powerUpTimer > 0)
                {
                    Debug.Log(powerUpTimer);
                    Debug.Log("PIERDE PODER");
                    powerUpTimer = 0; 
                    powerUpActivated = false;
                    invulnerableState = false;
                    barObject.SetActive(false);
                }
                
            }

            cooldown = changeCooldown;
        }
    }

    private void CheckPowerUpInput()
    {
        //If there are powerUp items and the power Up is not activated, the power-up activates
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
                currentVelocity = gameManager.getDefaultVelocity() * 1.5f;
                gameManager.ChangePlatformVelocity(currentVelocity);
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
    }

    private void PowerupUpdate()
    {
        if (powerUpActivated) //if (powerUpActivated && powerUpImage.enabled)
            {
                if (isTaco) //Taco Power-Up
                {
                    if (powerUpTimer <= 0)
                    {
                        powerUpTimer = 0;
                        currentVelocity = gameManager.getDefaultVelocity();
                        gameManager.ChangePlatformVelocity(currentVelocity);
                        powerUpActivated = false;
                        barObject.SetActive(false);
                    }
                    else
                    {
                        powerUpTimer -= Time.deltaTime;
                        progressBar.value -= Time.deltaTime;
                    }
                }
                else    //Tour Power-Up
                {   
                    if (powerUpTimer <= 0)
                    {
                        powerUpTimer = 0;
                        powerUpActivated = false;
                        invulnerableState = false;
                        barObject.SetActive(false);
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
                    currentVelocity = gameManager.getDefaultVelocity();
                    gameManager.ChangePlatformVelocity(currentVelocity);
                    gameManager.PauseCamera(false);
                    reSpawnTourTime = 2f;
                }
                else
                {
                    reSpawnTourTime -= Time.deltaTime;
                }
            }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            if(!invulnerableState)
                KillPlayer();
            else if(gameObject.transform.position.y < 0)
            {
                //pausar juego
                //lift Tour on a platform
                currentVelocity = 0;
                gameManager.ChangePlatformVelocity(0);
                gameManager.PauseCamera(true);
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
                                       GetComponent<SpriteRenderer>().bounds.size.y;
            
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
       // if (other.gameObject.tag == "Platform")
        if (other.gameObject.tag == "CrouchZone")
        {
            //if the player is below the platform
            //funciona por ahora, toca revisar player collider y size para saber cuando aplicarlo
            if (other.gameObject.transform.position.y > gameObject.GetComponent<Transform>().position.y)
            {
                //TODO: change it so it takes the bottom of the platform above it and subtracts from the y position
                //Check bounds
                transform.position = new Vector3(transform.position.x, -4.107f, transform.position.z);
                /*
                float newYPosition = other.gameObject.GetComponent<SpriteRenderer>().bounds.min.y 
                                     - gameObject.GetComponent<Transform>().localScale.y / 3;
                
                transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);*/
                
                Vector3 position = gameObject.GetComponent<Transform>().position;
                position = new Vector3(position.x - currentVelocity * Time.deltaTime, position.y, position.z);
                gameObject.GetComponent<Transform>().position = position;
            }
           
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
        GameObject gameOver = GameObject.Find("GameOver");
        //Debug.Log(gameOver);
        //TODO: make Game Over a canvas so "try again" button takes you to the correct level
        if(gameOver!=null)
           gameOver.SetActive(true);
        else 
            SceneManager.LoadScene("GameOver");
    }


    public void Pause(bool paused)
    {
        this.paused = paused;

        if (paused)
            currentVelocity = 0;
        else if (powerUpActivated && isTaco)
            currentVelocity = gameManager.getDefaultVelocity() * 1.5f;
        else
            currentVelocity = gameManager.getDefaultVelocity();
        
        gameManager.ChangePlatformVelocity(currentVelocity);
        
    }

    public Animator getAnimator()
    {
        return animator;
    }

    public void setCurrentVelocity(float currentVelocity)
    {
        this.currentVelocity = currentVelocity;
    }

}
