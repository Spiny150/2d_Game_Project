using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponManagment : MonoBehaviour
{
    GameObject holder;
    Vector2 holderPos;
    float angle;

    private void Awake() 
    {
        holder = gameObject.transform.Find("WeaponHolder").gameObject;
    }

    private void Update() {

        if (angle == 361) angle = 0;
        angle += 2f * Time.deltaTime;

        holderPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f + (Vector2)transform.position;

        holder.transform.position = holderPos;

        

        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        print(other.ClosestPoint(transform.position));
        if (!other.gameObject.name.Contains("Player")){

            Debug.DrawRay(other.ClosestPoint(transform.position), ((Vector2)transform.position - other.ClosestPoint(transform.position)));
        }
    }


    
}
