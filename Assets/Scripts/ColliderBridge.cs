using System.Collections.Generic;
using UnityEngine;

public class ColliderBridge : MonoBehaviour
{    
    public List<Collider2D> Colliders = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other) 
    {
        Colliders.Add(other);
    }
    private void OnTriggerExit2D(Collider2D other) 
    {
        Colliders.Remove(other);    
    }
}
