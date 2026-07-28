using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private float kickForce = 12f;
    [SerializeField] private float dribbleForce = 8f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float possessionDistance = 1.8f;

    private float kickDistance = 2.5f;
    private float dribbleDistance = 0.8f;
    private float frontAngleThreshold = 0.3f;

    public Rigidbody rb;
    private PlayerController possessingPlayer;
    private bool canKick = false;

    void Start()
    {
        RigidBodyInit();
        Reset();
    }

    private void RigidBodyInit()
    {
        rb = this.GetComponent<Rigidbody>();

        rb.mass = 0.45f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
    }

    void Update()
    {
        CheckPossesingPlayerDist();
    }

    private void CheckPossesingPlayerDist()
    {
        if (HasPossession())
        {
            if (Vector3.Distance(transform.position, possessingPlayer.transform.position) > possessionDistance)
            {
                canKick = false;
                possessingPlayer = null;
            }
        }
    }

    public void ExecuteKick(Vector3 direction, float forceMult = 1f)
    {
        if (!HasPossession()) return;

        float distToBall = Vector3.Distance(transform.position, possessingPlayer.transform.position);
        if (distToBall > kickDistance)
        {
            Vector3 toPlayer = (possessingPlayer.transform.position - transform.position).normalized;
            rb.AddForce(toPlayer * dribbleForce * 0.5f, ForceMode.Force);
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float finalForce = kickForce * forceMult;
        Vector3 kickDirection = (direction.normalized + Vector3.up * 0.2f).normalized;

        rb.AddForce(kickDirection * finalForce, ForceMode.Impulse);

        if (rb.linearVelocity.magnitude > maxSpeed) rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        ReleaseBall();
    }

    public void ReleaseBall()
    {
        canKick = false;
        possessingPlayer = null;
    }

    public void SetPossessingPlayer(PlayerController player)
    {
        possessingPlayer = player;
        canKick = true;
    }

    public void Dribble(Vector3 direction, float forceMult = 1f)
    {
        if (!HasPossession()) return;

        Vector3 dribbleDir = direction.normalized;
        Vector3 force = dribbleDir * dribbleForce * forceMult;
        Vector3 toPlayer = possessingPlayer.transform.position - transform.position;

        toPlayer.y = 0;

        if (toPlayer.magnitude > dribbleDistance) force += toPlayer.normalized * dribbleForce * 0.5f;

        rb.AddForce(force, ForceMode.Force);
    }

    public bool HasPossession()
    {
        if (!canKick || possessingPlayer == null) return false;

        Vector3 toBall = (transform.position - possessingPlayer.transform.position).normalized;
        float dot = Vector3.Dot(possessingPlayer.transform.forward, toBall);

        return dot > frontAngleThreshold;
    }

    public bool HasPossession(PlayerController player)
    {
        if (!canKick || possessingPlayer != player) return false;

        Vector3 toBall = (transform.position - player.transform.position).normalized;
        float dot = Vector3.Dot(player.transform.forward, toBall);

        return dot > frontAngleThreshold;
    }

    public TeamController GetTeamWithPossesion()
    {
        return HasPossession() ? possessingPlayer.team : null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !canKick)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.isActive) SetPossessingPlayer(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && possessingPlayer != null)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player == possessingPlayer) ReleaseBall();
        }
    }

    public void Reset()
    {
        possessingPlayer = null;
        canKick = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = new Vector3(102.1f, -1f, -70.2f);
    }
}