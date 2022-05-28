using UnityEngine;

public class PlayerCombat : MonoBehaviour
{

    GameObject weaponHitbox;
    Camera cam;

    private void Awake() {
        weaponHitbox = gameObject.transform.Find("weaponHitbox").gameObject;
        cam = Camera.main;
    }

    private void Update() {
        
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pointing = (mousePos - (Vector2) gameObject.transform.position).normalized;
        float angle = Vector2.SignedAngle(Vector2.up, pointing);

        weaponHitbox.transform.localPosition = new Vector2(pointing.x * 0.25f, pointing.y * 0.6f);
        weaponHitbox.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


    }

}
