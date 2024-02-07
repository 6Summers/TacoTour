using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = gameObject.GetComponent<Transform>().position;
        if(position.x < -20)
            Destroy(gameObject);
        else
        {
            position = new Vector3(position.x - speed * Time.deltaTime, position.y, position.z);
            gameObject.GetComponent<Transform>().position = position;
        }
        
    }
}
