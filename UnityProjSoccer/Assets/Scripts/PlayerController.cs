using UnityEngine;
public class PlayerController : MonoBehaviour
{

    public bool isActive = false;
    public float mouse_sens = 100f;

    private Rigidbody rb;
    private Vector3 movement_activation;
    private float xRot = 0f;
    
    [SerializeField]
    private float speed = 5.0f;


    void Start(){
        rb = this.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update(){


        movement_activation.x = Input.GetAxis("Horizontal");
        movement_activation.z = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        if(isActive) rb.linearVelocity = movement_activation*speed + new Vector3(0, rb.linearVelocity.y, 0);
    }
}
