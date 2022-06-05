using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPun
{

    #region movementInputVariable
    Vector2Int movement;
    bool isSprinting;

    #endregion movementInputVariable

    Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>()
    {
        {"up", KeyCode.Z},
        {"left", KeyCode.Q},
        {"down", KeyCode.S},
        {"right", KeyCode.D},
        {"sprint", KeyCode.LeftShift},
        {"attack", KeyCode.Mouse0}
    };
    

    private void Update()
    {
        if (photonView.IsMine)
        {
            movement = Vector2Int.zero;
            isSprinting = false;

            if (Input.GetKey(keys["up"]))
            {
                movement += Vector2Int.up;
            }
            if (Input.GetKey(keys["down"]))
            {
                movement += Vector2Int.down;
            }
            if (Input.GetKey(keys["left"]))
            {
                movement += Vector2Int.left;
            }
            if (Input.GetKey(keys["right"]))
            {
                movement += Vector2Int.right;
            }


            if (Input.GetKey(keys["sprint"]))
            {
                isSprinting = true;
            }
        }
    }

}
