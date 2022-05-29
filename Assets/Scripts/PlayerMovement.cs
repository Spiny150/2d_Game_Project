using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{

    PlayerCombat combat;

    Animator anim;
    Vector3 movement;
    Vector3 remotePos;
    Vector3 remoteVel;
    public Rigidbody2D rb;
    //float lag;

    const float baseSpeed = 4f;
    const float sprintSpeed = 6f;
    float speed = baseSpeed;
    int animatorDirection;
    bool isRunning = false;

    public bool stunned = false;
    public Vector2 slide = Vector2.zero;



    private void Awake() 
    {
        anim = GetComponentInChildren<Animator>();  
        rb = GetComponentInChildren<Rigidbody2D>();
        combat = gameObject.GetComponent<PlayerCombat>();
    }

    private void Update() 
    {
        if (photonView.IsMine)
        {
            movement = Vector3.zero;
            speed = baseSpeed;

            animatorDirection = 0;
            isRunning = false;

            if (Input.GetKey("z")) {
                movement += Vector3.up;
                animatorDirection = 2;
            }
            if (Input.GetKey("s")){
                movement += Vector3.down;
                animatorDirection = 4;
            }
            if (Input.GetKey("q")) {
                movement += Vector3.left;
                animatorDirection = 3;
            }
            if (Input.GetKey("d")) {
                movement += Vector3.right;
                animatorDirection = 1;
            }

            if (Input.GetKey(KeyCode.LeftShift)){
                speed = sprintSpeed;
                isRunning = true;
            }
        }
        else        
        {
            //transform.position = remotePos;
            rb.velocity = remoteVel;
            if (Vector2.Distance(transform.position, remotePos) > 0.5f || remoteVel == Vector3.zero)
            {
                transform.position = Vector2.Lerp(transform.position, remotePos, Time.deltaTime * 20f);
            }
            else if (Vector2.Distance(transform.position, remotePos) > 2f) transform.position = remotePos;
        }

        if (animatorDirection != anim.GetInteger("Direction")) anim.SetInteger("Direction", animatorDirection);
        if (isRunning != anim.GetBool("Running?")) anim.SetBool("Running?", isRunning);

    }

    private void FixedUpdate() 
    {
        if (photonView.IsMine)
        {
            if (slide.magnitude <= 10 / 3) stunned = false;
            if (slide.magnitude <= 0.1f) slide = Vector2.zero;
            if (stunned) movement = Vector2.zero;



            rb.velocity = movement * (speed / (slide.magnitude + 1));
            
            slide = Vector2.Lerp(slide, Vector2.zero, Time.deltaTime);

            rb.velocity += slide;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
            stream.SendNext(animatorDirection);
            stream.SendNext(isRunning);
        }
        else
        {
            remotePos = (Vector3)stream.ReceiveNext();
            remoteVel = (Vector2)stream.ReceiveNext();
            animatorDirection = (int)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();


            //lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (photonView.IsMine && other.gameObject.CompareTag("interactable")) other.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
    }    
}
