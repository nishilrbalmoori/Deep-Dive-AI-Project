using UnityEngine;

public class BallController : MonoBehaviour
{
     

    [Header("Ball Settings")]
    [SerializeField] private float  kickForce = 8f, currentKickForce = 0f, maxKickForce=20f, kickChargeSpeed = 15f;
    [SerializeField] private float dribbleForce = 8f;
    [SerializeField ]private float maxSpeed = 20f;

     [SerializeField] private float possessionDistance = 5f;

    private Vector3 lastVel;
    private Rigidbody rb;
    private PlayerController possessingPlayer;
    private bool canKick = false, isChargingKick = false;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        
        
        rb.mass = 0.45f; 
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;

    }

    void FixedUpdate()
    {
    
    }
    void Update()
    {
        CheckPossesingPlayerDist();
    }

    private void CheckPossesingPlayerDist()
    {
        if(HasPossession())
        {
            if(Vector3.Distance(transform.position, possessingPlayer.transform.position) > possessionDistance)
            {
                canKick = false;
                possessingPlayer = null;
            }
        }
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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float finalForce = kickForce * forceMult;
            Vector3 kickDirection = (direction.normalized + Vector3.up * 0.2f).normalized;

            rb.AddForce(kickDirection * finalForce, ForceMode.Impulse);

            if (rb.linearVelocity.magnitude > maxSpeed) rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

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
