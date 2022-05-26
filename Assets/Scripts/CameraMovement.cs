using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;

    private void Update() {

        if (player != null)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, player.transform.position.y, -10), Time.deltaTime * 3f);

            if (Input.GetKeyDown("a"))
            {
                transform.Rotate(0, 0, 90);
            }
            if (Input.GetKeyDown("e"))
            {
                transform.Rotate(0, 0, -90);
            }
        }

    }
}
