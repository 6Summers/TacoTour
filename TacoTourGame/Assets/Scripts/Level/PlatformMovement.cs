using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private float speed = 0;

    private Vector3 position;

    private Transform platformTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        platformTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        position = platformTransform.position;
        if(position.x < -21)
            Destroy(gameObject);
        else
        {
            position = new Vector3(position.x - speed * Time.deltaTime, position.y, position.z);
            platformTransform.position = position;
        }
        
    }
    
    //Change it so it's one script that controls all platforms
    
    public float GetSpeed() {
        return speed;
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }
}
