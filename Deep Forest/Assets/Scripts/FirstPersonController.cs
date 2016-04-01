using UnityEngine;
using System.Collections;


public class FirstPersonController : MonoBehaviour {

    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed;
    public bool showGridPosition = false;
    public Vector2 gridPosition;

    Game gameController;
    Transform cameraTransform;
    float verticalLookRotation;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    new Rigidbody rigidbody;

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        rigidbody = gameObject.AddComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	// Update is called once per frame
	void Update () {
        // Calculate the debug position
        gridPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        // Rotate the object using horizontal mouse input
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        // To limit the vertical rotation on the mouse we clamp the float value
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70, 70);
        // Rotate the camera verticaly
        cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

        // Calculate the amount of movement the rigidbody will be applied with
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetMoveAmount = moveDirection * walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, 0.15f);
	}

    void OnDrawGizmos()
    {
        if (showGridPosition)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(gridPosition.x, transform.position.y, gridPosition.y), Vector3.one);
        }
    }

    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    // Check for collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EndObjective")
        {
            gameController.NextLevel();
        }
        if (other.gameObject.tag == "Guard")
        {
            gameController.GameOver();
        }
    }
}
