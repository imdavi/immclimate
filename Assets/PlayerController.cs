using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int speed = 0;
    void Start()
    {

    }

    void Update()
    {
        // get input data from keyboard or controller
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        // update player position based on input
        Vector3 position = transform.position;
        position.x += moveHorizontal * speed * Time.deltaTime;
        position.z += moveVertical * speed * Time.deltaTime;
        transform.position = position;
    }
}
