using System.Collections;
using UnityEngine;

public class dummyScript : MonoBehaviour
{
    Rigidbody2D rb;

    Vector2 movement = Vector2.zero;
    Vector2 slide = Vector2.zero;
    const float speed = 5f;
    const float attackPunch = 10f;

    bool stunned = false;

    private void Awake() {
        rb = gameObject.GetComponent<Rigidbody2D>();
        StartCoroutine(baseMovement());
        gameObject.tag = "fightable";
    }

    public void GotHit(Vector2 from)
    {
        stunned = true;
        rb.velocity = Vector2.zero;
        slide = from.normalized * attackPunch;

    }

    IEnumerator baseMovement()
    {
        while(true)
        {
            movement = new Vector2((int) Random.Range(-1, 2), (int) Random.Range(-1, 2));
            yield return new WaitForSeconds(1);

        }    
    }

    private void Update() {
        
        if (slide.magnitude <= attackPunch / 3) stunned = false;
        else if (slide.magnitude <= 0.1f) slide = Vector2.zero;
        if (stunned) movement = Vector2.zero;



        rb.velocity = movement * (speed / (slide.magnitude + 1));
        
        slide = Vector2.Lerp(slide, Vector2.zero, Time.deltaTime);

        rb.velocity += slide;





    }
        
        
        
    
}
