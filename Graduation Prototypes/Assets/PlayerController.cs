using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    private Transform playerCameraParent;
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
    [FormerlySerializedAs("playerMesh")] private GameObject playerVisual;
    private float burrowVelocity;
    public float burrowAcceleration;
    public float burrowDeceleration;
    private float visualYPosition;
    private GameObject burrowParticle;
    private GameObject burrowParticle1;
    private GameObject burrowParticle2;
    private GameObject burrowParticle3;
    private GameObject burrowParticle4;
    public float burrowTurnRate;
    public float unburrowJump = 2f;
    public float maxTimingBoost = 5f;
    public float boostRange = 5f;
    public float burrowHeight = 10f;
    private Vector3 burrowDirection = Vector3.forward;

    private GameObject boostIndicator;

    public float slopeBoost = 10f;


    private bool unburrowing = false;
    private bool inAir = false;

    private void Awake()
    {
        playerCameraParent = GameObject.Find("Camera Parent").transform;
        playerVisual = GameObject.Find("Capsule");
        burrowParticle = GameObject.Find("BurrowParticle1");
        burrowParticle1 = GameObject.Find("BurrowParticle2");
        burrowParticle2 = GameObject.Find("BurrowParticle3");
        burrowParticle3 = GameObject.Find("BurrowParticle4");
        burrowParticle4 = GameObject.Find("BurrowParticle5");
        boostIndicator = GameObject.Find("BoostIndicator");
    }
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        visualYPosition = playerVisual.transform.localPosition.y;
        burrowParticle.SetActive(false);
        burrowParticle1.SetActive(false);
        burrowParticle2.SetActive(false);
        burrowParticle3.SetActive(false);
        burrowParticle4.SetActive(false);
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
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);

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

            if (hit.distance < burrowHeight)
                Burrow(true);
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

        if (!unburrowing && !inAir)
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



            if (hit.distance < boostRange && characterController.velocity.y < 0)
            {
                boostIndicator.SetActive(true);
            }
        }
        if (characterController.isGrounded && !unburrowing && !isBurrowed)
        {
            inAir = false;
            burrowVelocity = startBurrowSpeed;
        }
        unburrowing = false;
    }



    public void Burrow(bool boosting)
    {
        isBurrowed = true;
        burrowParticle.SetActive(true);
        //characterController.detectCollisions = false;

        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);

        float slope = 1 - hit.normal.y;
        Vector2 slopeDirection = new Vector2(hit.normal.x, hit.normal.z);
        Vector2 movementdir = new Vector2(transform.forward.x, transform.forward.z);
        float angle = Vector2.Angle(slopeDirection, movementdir);
        float boostMultiplier = Mathf.Lerp(1f, -1f, angle/180f);
        float boost = slopeBoost * slope * boostMultiplier;
        Debug.Log(boost);
        burrowVelocity += boost;



        if (boosting)
        {
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
            burrowVelocity = Mathf.Max(burrowVelocity, horizontalVelocity.magnitude * speed);
        }

        burrowVelocity = Mathf.Max(burrowVelocity, startBurrowSpeed);
        
        Vector3 meshPosition = playerVisual.transform.position;
        meshPosition.y = -2;
        playerVisual.transform.position = meshPosition;
        Debug.Log(burrowVelocity);
    }

    public void Unburrow()
    {
        isBurrowed = false;
        unburrowing = true;
        inAir = true;
        burrowParticle.SetActive(false);
        burrowParticle1.SetActive(false);
        burrowParticle2.SetActive(false);
        burrowParticle3.SetActive(false);
        burrowParticle4.SetActive(false);
        characterController.detectCollisions = true;
        
        Vector3 meshPosition = playerVisual.transform.localPosition;
        meshPosition.y = visualYPosition;
        playerVisual.transform.localPosition = meshPosition;
    }

    public void BurrowMovement()
    {
        if (burrowVelocity / maxBurrowSpeed > 0.4f)
            burrowParticle1.SetActive(true);
        else
            burrowParticle1.SetActive(false);
        if (burrowVelocity / maxBurrowSpeed > 0.55f)
            burrowParticle2.SetActive(true);
        else
            burrowParticle2.SetActive(false);
        if (burrowVelocity / maxBurrowSpeed > 0.70f)
            burrowParticle3.SetActive(true);
        else
            burrowParticle3.SetActive(false);
        if (burrowVelocity / maxBurrowSpeed > 0.85f)
            burrowParticle4.SetActive(true);
        else
            burrowParticle4.SetActive(false);


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
        //Debug.Log(burrowVelocity);
        if (!Input.GetButton("Jump"))
        {
            Unburrow();
        }
    }
}