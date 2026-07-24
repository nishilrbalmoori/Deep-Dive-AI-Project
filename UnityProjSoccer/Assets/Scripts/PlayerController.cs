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


    [Header("Movement")]
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float ballFollowSpeed = 2.5f;

    [SerializeField] private float followDistanceLimit = 10f;

    [SerializeField] private float dribbleCooldown = 0.2f;
    [SerializeField] private float mouseSens = 500f;


    private Rigidbody rb;
    private Vector3 movement_activation;
    private float rot = 0, speed = 5.0f;
    private float lastDribbleTime = 0f;
    private bool isIdle = true, isLeft = false, isRight = false, isSprinting = false, isChargingKick = false;    
    private int id;
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        
        rb = this.GetComponent<Rigidbody>();

        id = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
    }

    void Update(){
        if(isActive){
           float horiz = Input.GetAxis("Horizontal");
           
           PlayerInput(horiz);
           PlayerActiveAnimate(horiz);

        }

        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isLeft", isLeft);
        animator.SetBool("isRight", isRight);
        animator.SetBool("isSprinting", isSprinting);

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

    private void PlayerInput(float horiz)
    {
        
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isIdle;
        movement_activation = transform.forward * Input.GetAxis("Vertical") + transform.right * horiz;
        
        if(Input.GetKeyDown(KeyCode.Space) && ballController.HasPossession())
        {
            isChargingKick = true;
            ballController.StartKickCharge();
        }

        if(Input.GetKeyUp(KeyCode.Space) && isChargingKick)
        {
            isChargingKick = false;
            Vector3 kickDirection = movement_activation + transform.forward;

            if(kickDirection.sqrMagnitude < 0.01f) kickDirection = transform.forward;

            ballController.ExecuteKick(kickDirection);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && isChargingKick)
        {
            isChargingKick = false;
            ballController.CancelKick();
        }
         
    }

    private void PlayerActiveAnimate(float horiz)
    {
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

    private void PlayerControl()
    {

        rot += Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        rot %= 360f;

        
        if(isSprinting) {
            speed = sprintSpeed;

            if(movement_activation.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(movement_activation);
        }
        else {
            speed = runSpeed;
            transform.rotation = Quaternion.Euler(0, rot, 0);
        }

        rb.linearVelocity = movement_activation*speed + new Vector3(0, rb.linearVelocity.y, 0);

        if(ballController.HasPossession() && !isChargingKick) HandleDribbling();
        if(isChargingKick) ballController.UpdateKickCharge();
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
            
            if (toPlayer.magnitude > 1.2f)
            {
                ballController.Dribble(-toPlayer.normalized, 0.5f);
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
    }

    private void OnGUI()
    {
        if (isChargingKick && ballController.HasPossession())
        {
            float chargePercent = ballController.GetChargePercentage();
            GUI.Box(new Rect(Screen.width / 2 - 50, Screen.height - 50, 100, 20), "");
            GUI.Box(new Rect(Screen.width / 2 - 48, Screen.height - 48, 96 * chargePercent, 16), "");
            GUI.Label(new Rect(Screen.width / 2 - 30, Screen.height - 70, 100, 20), 
                $"Kick Power: {Mathf.Round(chargePercent * 100)}%");
        }
    }


}
