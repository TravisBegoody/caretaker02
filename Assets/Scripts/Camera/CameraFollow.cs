using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = player.position.x + offset.x;
        float y = player.position.y + offset.y;
        float z = player.position.z + offset.z;

        transform.position = new Vector3(x, y, z);
    }
}
