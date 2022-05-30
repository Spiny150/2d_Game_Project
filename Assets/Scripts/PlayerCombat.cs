using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCombat : MonoBehaviourPun
{

    GameObject weaponHitbox;
    Camera cam;
    PlayerMovement movement;
    public const float attackPunch = 10f;


    private void Awake() 
    {
        weaponHitbox = gameObject.transform.Find("weaponHitbox").gameObject;
        cam = Camera.main;
        movement = gameObject.GetComponent<PlayerMovement>();
        gameObject.tag = "Player";
    }

    private void Update() 
    {  
        if (photonView.IsMine)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 pointing = (mousePos - (Vector2) gameObject.transform.position).normalized;
            float angle = Vector2.SignedAngle(Vector2.up, pointing);

            weaponHitbox.transform.localPosition = new Vector2(pointing.x * 0.25f, pointing.y * 0.6f);
            weaponHitbox.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


            if (Input.GetMouseButtonDown(0))
            {
                List<Collider2D> targets = weaponHitbox.GetComponent<ColliderBridge>().Colliders;

                foreach (Collider2D target in targets)
                {
                    if (target.gameObject.CompareTag("Player")) target.gameObject.GetComponent<PlayerCombat>().GotHit(target.transform.position - gameObject.transform.position);
                }
            }

        }      
    }

    public void GotHit(Vector2 from)
    {
        if (photonView.IsMine)
        {
            movement.stunned = true;
            movement.rb.velocity = Vector2.zero;
            movement.slide = from.normalized * attackPunch;
        }

    }

}
