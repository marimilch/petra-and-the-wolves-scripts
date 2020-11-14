using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    public float moveFactor = 0.75f;

    private Vector3 offset;
    private float tilt;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        offset = transform.position - player.transform.position;
        tilt = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = player.transform.position;
        transform.position = playerPos * moveFactor + offset;

        //face player (flat)
        rotateToDirection(FlattenTo(playerPos - transform.position, 0));

        //tilt down
        //rotateToDirection(SlimTo(playerPos - transform.position, 0));
        transform.Rotate(Vector3.right, tilt);
    }

    void rotateToDirection(Vector3 dir)
    {
        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);

        transform.rotation = (
            Quaternion.AngleAxis(angle, Vector3.up)
        );
    }

    private Vector3 FlattenTo(Vector3 vec, float y)
    {
        return new Vector3(vec.x, y, vec.z);
    }

    private Vector3 SlimTo(Vector3 vec, float x)
    {
        return new Vector3(x, vec.y, vec.z);
    }
}
