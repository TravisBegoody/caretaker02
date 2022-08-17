using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapClamp : MonoBehaviour
{
    //Position it orginally was at
    Vector3 StartingPosition;
    public Transform playerPosition;

    public float clampSize = 25f;
    // Start is called before the first frame update
    void Start()
    {
        StartingPosition = this.transform.position;

        //playerPosition = GameManager.Instance.GetPlayer().transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float x = Mathf.Clamp(StartingPosition.x, playerPosition.position.x - clampSize, playerPosition.position.x + clampSize);
        float z = Mathf.Clamp(StartingPosition.z, playerPosition.position.z - clampSize, playerPosition.position.z + clampSize);

        this.transform.position = new Vector3(x,StartingPosition.y,z);
    }
}
