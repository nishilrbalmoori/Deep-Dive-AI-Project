using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public bool isActive = false;
   

    private Rigidbody rb;
    private Vector3 movement_activation;
    
    [SerializeField]
    private float speed = 5.0f;


    private Vector2 SCREEN_PIVOT = new Vector2(960, 0);


    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        
        rb = this.GetComponent<Rigidbody>();
    }

    void Update(){
        movement_activation = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        Vector2 mousePosAdj = Input.mousePosition;
        mousePosAdj -= SCREEN_PIVOT;

        float angle = (float)(Mathf.Atan2(mousePosAdj.x, mousePosAdj.y) * Mathf.Rad2Deg);

        if(mousePosAdj.y > 0) transform.rotation = Quaternion.Euler(0, angle, 0);
        if(isActive) rb.linearVelocity = movement_activation*speed + new Vector3(0, rb.linearVelocity.y, 0);

    }
}
