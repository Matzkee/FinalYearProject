using UnityEngine;
using System.Collections;


public class FirstPersonController : MonoBehaviour {

    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed;
    public bool showGridPosition = false;
    public Vector2 gridPosition;

    Transform cameraTransform;
    float verticalLookRotation;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    new Rigidbody rigidbody;

    // Use this for initialization
    void Start () {
        rigidbody = gameObject.AddComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	// Update is called once per frame
	void Update () {
        gridPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70, 70);
        cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

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
}
