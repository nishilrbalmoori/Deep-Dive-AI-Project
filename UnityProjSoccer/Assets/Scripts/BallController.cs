using UnityEngine;

public class BallController : MonoBehaviour
{
     

    [Header("Ball Settings")]
    [SerializeField] private float  kickForce = 0.1f, currentKickForce = 0f, maxKickForce=0.2f, kickChargeSpeed = 0.01f;
    [SerializeField] private float dribbleForce = 8f;
    [SerializeField] private float groundDrag = 0.98f, drag = 0.95f;
    [SerializeField ]private float maxSpeed = 15f;

    private Vector3 lastVel;
    private Rigidbody rb;
    private PlayerController possessingPlayer;
    private bool canKick = false, isChargingKick = false;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotation;

    }

    void FixedUpdate()
    {
        ApplyDrag();
    }
    void Update()
    {
        CheckPossesingPlayerDist();
    }

    private void CheckPossesingPlayerDist()
    {
        if(HasPossession())
        {
            if(Vector3.Distance(transform.position, possessingPlayer.transform.position) > 3f)
            {
                canKick = false;
                possessingPlayer = null;
            }
        }
    }

    private void ApplyDrag()
    {
        if(rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            
            float currDrag = (rb.linearVelocity.y < 0.1f && rb.linearVelocity.y > -0.1f) ? groundDrag : drag; 
            vel *= currDrag;

            if(vel.magnitude > maxSpeed) vel = vel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
        }

        lastVel = rb.linearVelocity;
    }

    public void StartKickCharge()
    {
        if(HasPossession())
        {
            isChargingKick = true;
            currentKickForce = kickForce;
        }
    }

    public void UpdateKickCharge()
    {
        if(isChargingKick && canKick)
        {
            currentKickForce += kickChargeSpeed * Time.deltaTime;
            currentKickForce = Mathf.Min(currentKickForce, maxKickForce);
        }
    }
    public void ExecuteKick(Vector3 direction, float forceMult = 1f)
    {
        if(HasPossession())
        {
            Vector3 kickDirection = direction.normalized;
            float finalForce = currentKickForce * forceMult;
            Vector3 upwardForce = Vector3.up * finalForce * 0.001f;

            rb.AddForce(kickDirection * finalForce + upwardForce, ForceMode.Impulse);

            ReleaseBall();
        }
    }

    public void ReleaseBall()
    {
        CancelKick();
        canKick = false;
        possessingPlayer = null;
    }

    public void CancelKick()
    {
        isChargingKick = false;
        currentKickForce = kickForce;
    }
    public void SetPossessingPlayer(PlayerController player)
    {
        possessingPlayer = player;
        canKick = true;
    }

    public void Dribble(Vector3 direction, float forceMult = 1f)
    {
        if(HasPossession())
        {
            Vector3 dribbleDir = direction.normalized;
            Vector3 force = dribbleDir * dribbleForce * forceMult;
            Vector3 toPlayer = possessingPlayer.transform.position - transform.position;
            
            toPlayer.y = 0;

            if(toPlayer.magnitude > 1.5f) force += toPlayer.normalized * dribbleForce * 0.5f;

            rb.AddForce(force, ForceMode.Force);
        }
    }

    public bool HasPossession()
    {
        return canKick && possessingPlayer != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !canKick)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.isActive)
            {
                SetPossessingPlayer(player);
            }
        }
    }

    public bool IsChargingKick()
    {
        return isChargingKick;
    }

    public float GetChargePercentage()
    {
        if (!isChargingKick) return 0f;
        return (currentKickForce - kickForce) / (maxKickForce - kickForce);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && possessingPlayer != null)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player == possessingPlayer)
            {
                ReleaseBall();
            }
        }
    }
}
