using TMPro;
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
    public float startBurrowSpeed = 1f;

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
    public float unburrowJump = 2f;
    public float maxTimingBoost = 5f;
    public float boostRange = 5f;
    private Vector3 burrowDirection = Vector3.forward;

    public GameObject boostIndicator;

    private bool unburrowing = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        visualYPosition = playerVisual.transform.localPosition.y;
        burrowParticle.SetActive(false);
        burrowVelocity = startBurrowSpeed;
    }

    void Update()
    {
        boostIndicator.SetActive(false);
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
        if (characterController.isGrounded && canMove && !unburrowing)
        {
            moveDirection.y = 0f;
            if (Input.GetButtonDown("Jump"))
            {
                Burrow(false);
            }
        }

        if (Input.GetButtonDown("Jump") && !unburrowing && !characterController.isGrounded)
        {
            Burrow(true) ;
        }

        
        if (unburrowing)
        {
            burrowDirection.y = unburrowJump;
        }

        else
        {
            float curSpeedY = Input.GetAxis("Vertical");
            float curSpeedX = Input.GetAxis("Horizontal");
            Vector2 moveDirection2 = new Vector2(curSpeedX, curSpeedY).normalized;
            moveDirection.x = moveDirection2.x;
            moveDirection.y -= gravity * Time.deltaTime;
            moveDirection.z = moveDirection2.y;
        }
        moveDirection = Quaternion.AngleAxis(rotation.y, Vector3.up) * moveDirection;
        if (characterController.isGrounded && !unburrowing)
        {
            characterController.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            if (burrowVelocity < startBurrowSpeed)
            {
                burrowVelocity = startBurrowSpeed;
            }
            burrowDirection.y -= gravity * Time.deltaTime;
            characterController.Move(new Vector3(burrowDirection.x * burrowVelocity * Time.deltaTime,
                                                 burrowDirection.y * Time.deltaTime,
                                                 burrowDirection.z * burrowVelocity * Time.deltaTime));
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit);
            if (hit.distance < boostRange && characterController.velocity.y < 0)
            {
                boostIndicator.SetActive(true);
            }
        }
        unburrowing = false;
        
    }

    public void Burrow(bool boosting)
    {
        isBurrowed = true;
        burrowParticle.SetActive(true);
        //characterController.detectCollisions = false;
        

        if(boosting)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit);
            Debug.Log(hit.distance);
            //float boostMultiplier = Mathf.Max(0f, 1 - hit.distance / boostRange);
            //burrowVelocity += boostMultiplier * maxTimingBoost;
            //burrowVelocity += maxTimingBoost;
            if (hit.distance < boostRange && characterController.velocity.y < 0)
            {
                burrowVelocity += maxTimingBoost;
            }
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(moveDirection.x, 0f, moveDirection.z);
            burrowVelocity = horizontalVelocity.magnitude * speed;
        }

        burrowVelocity = Mathf.Max(burrowVelocity, startBurrowSpeed);
        
        Vector3 meshPosition = playerVisual.transform.position;
        meshPosition.y = -2;
        playerVisual.transform.position = meshPosition;
    }

    public void Unburrow()
    {
        isBurrowed = false;
        unburrowing = true;
        burrowParticle.SetActive(false);
        characterController.detectCollisions = true;
        
        Vector3 meshPosition = playerVisual.transform.localPosition;
        meshPosition.y = visualYPosition;
        playerVisual.transform.localPosition = meshPosition;
    }

    public void BurrowMovement()
    {
        //if (Input.GetKey(KeyCode.W))
        //{
            burrowVelocity = Mathf.Min(burrowVelocity + burrowAcceleration, maxBurrowSpeed);
        //}
        //else
        //{
        //    burrowVelocity = Mathf.Max(burrowVelocity - burrowDeceleration, 0f);
        //}
        Vector3 targetBurrowDirection = transform.forward;
        burrowDirection = characterController.velocity.normalized;
        if (burrowDirection.sqrMagnitude < 1f)
        {
            burrowDirection = transform.forward;
        }
        burrowDirection = Vector3.Lerp(burrowDirection, targetBurrowDirection, Time.deltaTime * burrowTurnRate);
        burrowDirection.y = -gravity;
        characterController.Move(burrowDirection * burrowVelocity * Time.deltaTime);
        
        if (!Input.GetButton("Jump"))
        {
            Unburrow();
        }
        Debug.Log(burrowVelocity);
    }
}