using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Player Settings")]
    public bool isActive = false;
   public Transform ball;
   public Animator animator;

    public BallController ballController;

    public GameObject playerPositionings;

    public BoxCollider collider;

    [Header("Movement")]
    public const float sprintSpeed = 7.5f;
    public const float runSpeed = 5.0f;
    public const float ballFollowSpeed = 2.5f;

    [SerializeField] private float followDistanceLimit = 10f;

    [SerializeField] private float dribbleCooldown = 0.2f;
    [SerializeField] private float mouseSens = 500f;

    [Header("ML Settings")]
    public bool useML = false;  

    [Header("Performance")]
    public bool isTraining = false; 
    public string role;
    public int rolePosition;

    public TeamController team;

    public Rigidbody rb;
    private Vector3 movement_activation;
    private float rot = 0, speed = 5.0f;
    private float lastDribbleTime = 0f;
    private bool isIdle = true, isLeft = false, isRight = false, isSprinting = false;
    public bool withinSoftBoundaries = true;  
    public int id;

    public float ballFollowDistance = 10f;
    public const float ballDribbleDistance = 1.2f;

    private Vector3 lastPos;

    void Awake()
    {
        Init();
    }
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Init()
    {
        rb = this.GetComponent<Rigidbody>();
        collider = this.GetComponent<BoxCollider>();
        id = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);

    }

    void Update(){
        if(isActive){
            if(!useML){
                float horiz = Input.GetAxis("Horizontal");
                
                PlayerInput(horiz);
                PlayerActiveAnimate(horiz);
            }
            else UpdateMLAnimation();

        }

        UpdateAnimator();
    }

    void UpdateMLAnimation(){
        if(!isTraining){
            float speed = rb.linearVelocity.magnitude;
            isIdle = speed < 0.5f;
            isSprinting = speed > 6f;
            isLeft = false;
            isRight = false;
        }
    }


    private void UpdateAnimator()
    {
        if(!isTraining){
            animator.SetBool("isIdle", isIdle);
            animator.SetBool("isLeft", isLeft);
            animator.SetBool("isRight", isRight);
            animator.SetBool("isSprinting", isSprinting);
        }
    }
    void FixedUpdate()
    {
        if (isActive)
        {
            if(!useML) PlayerControl();
        }
        else if(withinSoftBoundaries && Vector3.Distance(transform.position, ball.position) > followDistanceLimit)
            BallFollow();
        else if(!withinSoftBoundaries)
        {
            Reset();
            withinSoftBoundaries = true;
        }
        else
        {
            isIdle = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void PlayerInput(float horiz)
    {
        
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isIdle;
        movement_activation = transform.forward * Input.GetAxis("Vertical") + transform.right * horiz;
        
        if(Input.GetKeyDown(KeyCode.Space) && ballController.HasPossession())
        {
            Vector3 kickDirection = movement_activation + transform.forward;
            if (kickDirection.sqrMagnitude < 0.01f) kickDirection = transform.forward;
            ballController.ExecuteKick(kickDirection);
        }
    }

    private void PlayerActiveAnimate(float horiz)
    {
        if(!isTraining){
            if(!isSprinting){
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
            }
            else
            {
                isLeft = false;
                isRight = false;
                isIdle = false;
            }
        }
    }

    private void PlayerControl()
    {

        rot += Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        rot %= 360f;

        speed = (isSprinting) ? sprintSpeed : runSpeed;
        transform.rotation = Quaternion.Euler(0, rot, 0);

        rb.linearVelocity = movement_activation*speed + new Vector3(0, rb.linearVelocity.y, 0);

        if(ballController.HasPossession()) HandleDribbling();
    }

    private void HandleDribbling(){
        if(movement_activation.sqrMagnitude > 0.01f && Time.time > lastDribbleTime + dribbleCooldown)
        {
            Vector3 dribbleDir = movement_activation.normalized;
            float forceMult = isSprinting ? 1.2f : 0.8f;

            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-0.1f, 0.1f),
                0,
                UnityEngine.Random.Range(-0.1f, 0.1f)
            );

            ballController.Dribble(dribbleDir + randomOffset, forceMult);
            lastDribbleTime = Time.time;
        }
        if (movement_activation.magnitude < 0.1f && ballController.HasPossession())
        {
            Vector3 toPlayer = transform.position - ball.position;
            toPlayer.y = 0;
            
            if (toPlayer.magnitude > ballDribbleDistance)
            {
                ballController.Dribble(-toPlayer.normalized);
            }
        }
    }

    private void BallFollow()
    {
        Vector3 direction = ball.position - transform.position;
        direction.y = 0;

        isLeft = false;
        isRight = false;
        isSprinting = false;

        if(direction.magnitude > followDistanceLimit)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            rb.linearVelocity = transform.forward * ballFollowSpeed;
            isIdle = false;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            isIdle = true;
        }
        lastPos = transform.position;
    }

    public void Reset()
    {
        rb.linearVelocity = Vector3.zero;
        movement_activation = Vector3.zero;
        rot = 0;

        foreach(Transform playingPosition in playerPositionings.transform){

            if(playingPosition.name == $"{role} {rolePosition}"){
                foreach(Transform child in playingPosition){
                    transform.position = child.position;
                    if (child.TryGetComponent<PositioningController>(out PositioningController script))
                        script.playerController = this;
                }
            }
        }

        withinSoftBoundaries = true;
        isIdle = true; isLeft = false; isRight = false; isSprinting = false; 
        
        UpdateAnimator();
    }
}