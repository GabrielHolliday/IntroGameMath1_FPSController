using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThisIsTheOneUsingTheCharachterController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //
    private CharacterController charController;
    private Camera cam;
    private (float, float) minMaxPitchLook = (-250f, 250f);
    private Vector3 impliedMoveDir = Vector3.zero;
    private Vector3 velocity = Vector3.zero;//for b hopping and smoothing and whatnot, what actually moves the player
    private float crouchDist = 0.9f;
    private Vector3 targCamPos;
    private bool canStandup;
    public GameObject RespawnPoint;
    public enum PlrState
    {
        Sprinting,
        OnGround,
        FreeFall,
        Tumble,
        Crouching,
    }
    public PlrState plrState = PlrState.OnGround;
    //
    public InputActionReference move;
    public InputActionReference look;
    public InputActionReference jump;
    public InputActionReference sprint;
    public InputActionReference crouch;

    public float yLookSensitivity;
    public float xLookSensitivity;
    public float airControl;
    public float groundControl;
    public float gravity;
    public float speed;
    public float sprintSpeed;
    public float jumpHeight;
    public float cameraHeight;

    private float Clamp(float num, float maxNum, float minNum)
    {
        if (num > maxNum) return maxNum;
        else if (num < minNum) return minNum;
        return num;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (plrState == PlrState.FreeFall | plrState == PlrState.FreeFall) return;
        Debug.Log("hee hoo");
        impliedMoveDir = Vector3.up * jumpHeight;
        velocity = Vector3.Lerp(velocity, impliedMoveDir, 0.01f);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        charController = transform.GetComponent<CharacterController>();
        cam = transform.Find("Camera").GetComponent<Camera>();

        jump.action.started += Jump;
    }

    // Update is called once per frame
    private float pitch;
    private float yaw;
    void Update()
    {
        bool lazyBool = false;
        if(crouch.action.ReadValue<float>() !=0f || canStandup == false)
        {  
            lazyBool = true;
            charController.height = 2 - crouchDist;
            //charController.center = new Vector3(0,crouchDist,0);
            transform.localScale = new Vector3(1,0.2f,1);
            targCamPos = new Vector3(cam.transform.localPosition.x,cameraHeight - crouchDist,cam.transform.localPosition.z);

            //roof check
            RaycastHit roofHit;
            Physics.Raycast(transform.position, Vector3.up, out roofHit, charController.height + charController.skinWidth + 0.1f); //0.1f is a buffer
            if(roofHit.distance != 0)
            {
                canStandup = false;
            }
            else
            {
                canStandup = true;
            }
            //
        }
        else
        {
            charController.height = 2;
            targCamPos = new Vector3(cam.transform.localPosition.x,cameraHeight,cam.transform.localPosition.z); 
            transform.localScale = new Vector3(1,1,1);
            
            //charController.center = new Vector3(0,0,0);
        }
        //moving (coppied from other charachter controller (the one that i made in the same project))
        RaycastHit floorHit;
        Physics.Raycast(transform.position, Vector3.down, out floorHit, charController.height - charController.skinWidth- 0.1f);
        if (charController.isGrounded && Vector3.Angle(floorHit.normal, Vector3.up) <= charController.slopeLimit)
        {
            if (sprint.action.ReadValue<float>() != 0f && lazyBool == false)
            {
                plrState = PlrState.Sprinting;
            }
            else
            {
                plrState = PlrState.OnGround;   
            }
        }
        else if (plrState != PlrState.Tumble)
        {
            //clampPitch = false;
            
            plrState = PlrState.FreeFall;
        }
        
        
        Debug.Log(targCamPos);
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targCamPos, Clamp(Time.deltaTime, 1f,0.01f));
        
        
        
        impliedMoveDir = Vector3.zero;

        float localSpeed = 0;
        float control = groundControl;
        switch(plrState)
        {
            case PlrState.OnGround:
                localSpeed = speed;
                break;
            case PlrState.Sprinting:
                localSpeed = sprintSpeed;
                break;
            case PlrState.FreeFall:
                impliedMoveDir = velocity;
                localSpeed = speed;
                control = airControl;
                break;
            case PlrState.Tumble:
                localSpeed = 0;
                control = 0;
                break;
            case PlrState.Crouching:
                localSpeed = 0;
                control = 0;
                break;
        }
        
        impliedMoveDir += transform.forward * move.action.ReadValue<Vector2>().y * ((float)localSpeed);
        impliedMoveDir += transform.right * move.action.ReadValue<Vector2>().x * ((float)localSpeed);


        velocity = Vector3.Lerp(velocity, impliedMoveDir, control * Time.deltaTime);
        

        RaycastHit hitTwo;
        Physics.SphereCast(transform.position, 1f, impliedMoveDir, out hitTwo, 0.2f);
        if(hitTwo.distance !=0 && Vector3.Angle(hitTwo.normal, Vector3.up) > charController.slopeLimit)//bonk
        {
            float bounceOffAngle = 0;
            float bounceOffSpeedLoss = 1;
            bounceOffAngle = Vector3.Angle(hitTwo.normal, Vector3.up);
            bounceOffSpeedLoss = 0.3F;

            velocity = Vector3.Reflect(velocity, hitTwo.normal);
            velocity = velocity * bounceOffSpeedLoss;

            

            Debug.Log($"{bounceOffAngle}, {bounceOffSpeedLoss}");
        }

        if (plrState == PlrState.FreeFall)
        {
            impliedMoveDir = transform.up * -gravity;
            if(Vector3.Angle(floorHit.normal, Vector3.up) > charController.slopeLimit)
            {
                Debug.Log("eieio");
                impliedMoveDir = Vector3.Reflect(impliedMoveDir, floorHit.normal);
                impliedMoveDir += floorHit.normal * (50f -floorHit.distance); //player was sticking to slopes, so this is bonking them off
            }
            //Debug.Log("e");
        }
        

        velocity = Vector3.Lerp(velocity, impliedMoveDir, Time.deltaTime * 0.1f);
        
        charController.Move(velocity * Time.deltaTime);


        //looking
        pitch += look.action.ReadValue<Vector2>().y;
        yaw += look.action.ReadValue<Vector2>().x;

        pitch = Clamp(pitch, minMaxPitchLook.Item2, minMaxPitchLook.Item1);

        cam.transform.localRotation = quaternion.Euler(pitch * -0.01f * yLookSensitivity, 0, 0);
        transform.rotation = quaternion.Euler(0, yaw * 0.01f * xLookSensitivity, 0);

        if(transform.position.y < -50)
        {
            transform.position = RespawnPoint.transform.position;
        }
    }
}
