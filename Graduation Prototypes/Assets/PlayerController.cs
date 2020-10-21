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
    public float maxBurrowSpeed = 40f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    [HideInInspector]
    public bool canMove = true;

    public bool isBurrowed = false;
    [FormerlySerializedAs("playerMesh")] public GameObject playerVisual;
    private float burrowVelocity;
    public float burrowAcceleration;
    public float burrowDeceleration;
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
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.eulerAngles = new Vector2(0, rotation.y);
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
            Vector3 horizontalMoveDirection = (forward * curSpeedX) + (right * curSpeedY);
            moveDirection.x = horizontalMoveDirection.x;
            moveDirection.z = horizontalMoveDirection.z;

            if (Input.GetButton("Jump") && canMove)
            {
                Burrow();
            }
        }


        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    public void Burrow()
    {
        isBurrowed = true;
        burrowParticle.SetActive(true);
        //characterController.detectCollisions = false;
        Vector3 horizontalVelocity = new Vector3(moveDirection.x, 0f, moveDirection.z) * Time.deltaTime;
        burrowVelocity = horizontalVelocity.magnitude;
        
        
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
        moveDirection.y = characterController.velocity.y;
    }

    public void BurrowMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            burrowVelocity = Mathf.Min(burrowVelocity + burrowAcceleration * Time.deltaTime, maxBurrowSpeed);
        }
        else
        {
            burrowVelocity = Mathf.Max(burrowVelocity - burrowDeceleration * Time.deltaTime, 0f);
        }
        Vector3 targetBurrowDirection = transform.forward * burrowVelocity;
        Vector3 burrowDirection = characterController.velocity.normalized;
        burrowDirection = Vector3.Lerp(burrowDirection, targetBurrowDirection, Time.deltaTime * burrowTurnRate);
        burrowDirection.y = targetBurrowDirection.y;
        burrowDirection.y -= gravity * Time.deltaTime;
        characterController.Move(burrowDirection * burrowVelocity);
        
        if (!Input.GetButton("Jump"))
        {
            Unburrow();
        }
    }
}