using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FPController : MonoBehaviour
{
    private CharacterController characterController;
    public float walkSpeed = 1;
    public float SprintSpeed = 3;
    private float CurrentSpeed;
    public float jumpForce = 5;
    public float mouseSensitivity = 2;
    float verticalRotation;
    private float gravity = 9.81f;
    private Vector3 currentMovement;
    float upDownRange = 80;
    public float ThrowForce = 50;
    private Item heldItem;
    public Transform HoldPoint;
    private Vector3 hitPoint;
    public ParticleSystem impactPS;
    public int ParticleCount = 15;
    public float PickupRange = 2;
    private Camera cam;
    private AudioSource audioSource;
    float walkStepInterval = 0.5f;
    float runStepInterval = 0.3f;
    float currentStepInterval;
    bool isMoving;
    bool isSprinting;
    float nextTimeStep;
    float velocityThreshold = 2;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
        audioSource = GetComponent<AudioSource>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        Movement();
        MouseLook();
        Sprinting();
        Jumping();

        if (heldItem != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                heldItem.Throw(ThrowForce, cam.transform.forward);
                heldItem = null;
            }
        }

        if (ObjectInFocus() != null)
        {
            float distanceToObject = Vector3.Distance(cam.transform.position, ObjectInFocus().transform.position);
            print(ObjectInFocus().name);
            if (Input.GetMouseButtonDown(0))
            {
                impactPS.transform.position = hitPoint;
                impactPS.Emit(ParticleCount);
            }

            if (distanceToObject <= PickupRange && ObjectInFocus().GetComponent<Item>() != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    heldItem = ObjectInFocus().GetComponent<Item>();
                    heldItem.PickUp(cam.transform, HoldPoint.position);
                }
            }
        }
    }

    void Sprinting()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            CurrentSpeed = SprintSpeed;
            isSprinting = true;
        } else
        {
            CurrentSpeed = walkSpeed;
            isSprinting = false;
        }

    }
    void Movement()
    {
        float verInput = Input.GetAxis("Vertical");
        float horInput = Input.GetAxis("Horizontal");
        float verSpeed = verInput * CurrentSpeed;
        float horSpeed = horInput * CurrentSpeed;

        Vector3 horizontalMovement = new Vector3(horSpeed, 0, verSpeed);
        horizontalMovement = transform.rotation * horizontalMovement;
        currentMovement.x = horizontalMovement.x;
        currentMovement.z = horizontalMovement.z;

        characterController.Move(currentMovement * Time.deltaTime);
        isMoving = verInput != 0 || horInput != 0;
    }
    void Jumping()
    {
        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }
    void MouseLook()
    {
        float mouseXrotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseXrotation, 0);
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    public GameObject ObjectInFocus()
    {
        GameObject result = null;
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
        {
            result = hit.transform.gameObject;
            hitPoint = hit.point;
        }
        return result;

    }
    private void HandleFootsteps()
    {
        currentStepInterval = isSprinting? runStepInterval: walkStepInterval;

        if (characterController.isGrounded && isMoving && Time.time > nextTimeStep && characterController.velocity.magnitude > velocityThreshold)
        {
            AudioManager.inst.PlayFootstep(audioSource);
            nextTimeStep = Time.time + currentStepInterval;

        }
    }
}





