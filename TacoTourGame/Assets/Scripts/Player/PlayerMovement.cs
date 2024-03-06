using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private float jumpForce = 50f;
    
    
    private Rigidbody2D rigidbody;
    private Animator animator;
    
    //enum for checking character state
    private enum PlayerState {Running,  Crouching, Reaching, Clinging, Falling, Stunned}
    private PlayerState playerState = PlayerState.Running;
    
    [Header("Character Change Variables")]
    [SerializeField] private float changeCooldown =1.2f;
    private float cooldown;
    [SerializeField] private bool isTaco = true;

    [Header("Jumps")] 
    [SerializeField] private float catJump;
    [SerializeField] private float dogJump;
    [SerializeField] private float speedCollision = 5;          //When the character is stucked under a platform
    private bool isActionActive = false;                        //This variable is to know if the dog is dizzy
    [SerializeField] private float dizzyDog = 1;               //Dizy time 
    private float actionTimer = 0f;                             //Timer to track the elapsed time of the action
    private int ceilingCollisionCount = 0;                      //to control when the character is collisioning with the ceiling
    void Start()
    {
        rigidbody =  gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        
        animator.SetBool("isTaco", isTaco);
        jumpForce = dogJump;
    }
/*
    void FixedUpdate()
    {
        rigidbody.velocity = new Vector2(speed, rigidbody.velocity.y);
    }
*/
    void Update()
    {
        if(cooldown>0) cooldown--;
        
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
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

            if (!isTaco) jumpForce = catJump;
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

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            KillPlayer();
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
            
                Debug.Log("counter " + ceilingCollisionCount);

                playerState = PlayerState.Clinging;
            }
            
        }
        else if (other.CompareTag("EndZone"))
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
        if (other.gameObject.tag == "CrouchZone")
        {
            transform.position = new Vector3(transform.position.x, -4.107f, transform.position.z);
            Debug.Log("on collision stay ");
        
            Vector3 position = gameObject.GetComponent<Transform>().position;
            position = new Vector3(position.x - speed * Time.deltaTime, position.y, position.z);
            gameObject.GetComponent<Transform>().position = position;
        }
    }

    bool IsGrounded()
    {
        return rigidbody.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    void Jump()
    {
        //rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        rigidbody.velocity = new Vector2(0, jumpForce);
        
    }

    private void KillPlayer()
    {
        Destroy(this.GameObject());
        SceneManager.LoadScene("GameOver");
    }
    
}
