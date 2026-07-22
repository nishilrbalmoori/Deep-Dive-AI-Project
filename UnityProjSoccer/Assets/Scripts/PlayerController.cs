using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public bool isActive = false    ;
   public Transform ball;
   public Animator animator;

    private Rigidbody rb;
    private Vector3 movement_activation;


    private bool isIdle = true, isLeft = false, isRight = false;    



    [SerializeField]
    private float sprintSpeed = 7.5f;
    private float speed = 5.0f;
    private float ballFollowSpeed = 2.5f;

    private float followDistanceLimit = 10f;

    private Vector2 SCREEN_PIVOT = new Vector2(960, 0);


    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        
        rb = this.GetComponent<Rigidbody>();
    }

    void Update(){
        if(isActive){
           float horiz = Input.GetAxis("Horizontal");
           
           PlayerInput(horiz);
           PlayerActiveAnimate(horiz);
        }
        else
        {
            NotActiveAnimate();
        }

    }

    void FixedUpdate()
    {
        if(isActive){
            PlayerControl();
        }
        else
        {
            BallFollow();
        }
    }

    private void NotActiveAnimate()
    {
        isLeft = false;
        isRight = false;

        if(rb.linearVelocity.magnitude > 0) isIdle = false;
        else isIdle = true;
    }

    private void PlayerInput(float horiz)
    {
        movement_activation = transform.forward * Input.GetAxis("Vertical") + transform.right * horiz;
    }

    private void PlayerActiveAnimate(float horiz)
    {
        if(movement_activation.magnitude > 0) {
            isIdle = false;

            if(horiz > 0) isRight = true;
            else if(horiz < 0) isLeft = true;

        }
        else isIdle = true;

        if(horiz == 0)
        {
            isLeft = false;
            isRight = false;
        }

        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isLeft", isLeft);
        animator.SetBool("isRight", isRight);
    }

    private void PlayerControl()
    {
        Vector2 mousePosAdj = Input.mousePosition;
        mousePosAdj -= SCREEN_PIVOT;

        float angle = (float)(Mathf.Atan2(mousePosAdj.x, mousePosAdj.y) * Mathf.Rad2Deg);

        if(mousePosAdj.y > 0) transform.rotation = Quaternion.Euler(0, angle, 0);
        rb.linearVelocity = movement_activation*speed + new Vector3(0, rb.linearVelocity.y, 0);
    }

    private void BallFollow()
    {
        Vector3 direction = ball.position - transform.position;
        direction.y = 0;

        if(direction.magnitude > followDistanceLimit)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            rb.linearVelocity =  transform.forward * ballFollowSpeed;   
        }
    }

    private void IsActiveAnim(){}
}
