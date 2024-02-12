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
    
    private bool isActionActive = false;                        //This variable is to know if the dog is dizzy
    [SerializeField] private float dizzyDog = 1;               //Dizy time 
    private float actionTimer = 0f;                             //Timer to track the elapsed time of the action
    
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
            animator.SetTrigger("isCrouching");
        }

        if (Input.GetKeyDown((KeyCode.K)) && cooldown <= 0) //add cooldown check
        {
            isTaco = !isTaco;
            
            animator.SetBool("isTaco", isTaco);

            if (!isTaco) jumpForce = catJump;
            else jumpForce = dogJump;
            
            cooldown = changeCooldown;
            
        }
        
        if (Input.GetKeyDown(KeyCode.A) && !isTaco && playerState != PlayerState.Clinging && IsGrounded())
        {
            Jump();
            playerState = PlayerState.Reaching;
            animator.SetTrigger("isReaching");
            
            Debug.Log("Reaching");

        }
        
        if ((Input.GetKeyUp(KeyCode.A) || isTaco) && playerState == PlayerState.Clinging) //if the player stops clinging or if they change to taco
        {
            rigidbody.gravityScale = 1.3f;
            playerState = PlayerState.Falling;
            animator.SetTrigger("isFalling");
            
            Debug.Log("Falling");

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
        else if (other.CompareTag("Ceiling") && playerState == PlayerState.Reaching)
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
            
            Debug.Log(characterPosition.y);

            playerState = PlayerState.Clinging;
        }
        else if (other.CompareTag("EndZone"))
        {
            Debug.Log("theEnd");
            Destroy(this.GameObject());
            SceneManager.LoadScene("EndTutorial");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("on trigger exit");
        if (other.CompareTag("Ceiling") && playerState == PlayerState.Clinging)
        {
            rigidbody.gravityScale = 1.3f;
            playerState = PlayerState.Falling;
            animator.SetTrigger("isFalling");
            Debug.Log("no mas ceilling");
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
