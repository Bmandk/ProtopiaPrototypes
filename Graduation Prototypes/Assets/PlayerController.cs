using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    [HideInInspector]
    public bool canMove = true;

    public bool isBurrowed = false;
    [FormerlySerializedAs("playerMesh")] public GameObject playerVisual;
    private float burrowVelocity;
    public float burrowAcceleration;
    private float visualYPosition;
    public GameObject burrowParticle;
    public float burrowTurnRate;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        visualYPosition = playerVisual.transform.localPosition.y;
        burrowParticle.SetActive(false);
    }

    void Update()
    {
        if (isBurrowed)
            BurrowMovement();
        else
            GroundMovement();
    }

    public void GroundMovement()
    {
        if (characterController.isGrounded)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButton("Jump") && canMove)
            {
                Burrow();
            }
        }


        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
        }
    }

    public void Burrow()
    {
        isBurrowed = true;
        burrowParticle.SetActive(true);
        //characterController.detectCollisions = false;
        burrowVelocity = 0;
        
        Vector3 meshPosition = playerVisual.transform.position;
        meshPosition.y = -2;
        playerVisual.transform.position = meshPosition;
    }

    public void Unburrow()
    {
        isBurrowed = false;
        burrowParticle.SetActive(false);
        characterController.detectCollisions = true;
        
        Vector3 meshPosition = playerVisual.transform.localPosition;
        meshPosition.y = visualYPosition;
        playerVisual.transform.localPosition = meshPosition;
    }

    public void BurrowMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float dir = Input.GetAxisRaw("Horizontal");
        forward = Quaternion.Euler(0, dir, 0) * (forward * (Time.deltaTime * burrowTurnRate));
        forward.Normalize();
        transform.forward = forward;
        
        burrowVelocity += burrowAcceleration * Time.deltaTime;
        characterController.Move(forward * burrowVelocity);
        
        if (!Input.GetButton("Jump"))
        {
            Unburrow();
        }
    }
}