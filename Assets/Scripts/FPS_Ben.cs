using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class FPS_Ben : MonoBehaviour
{
    //public stuff
    public float speed;
    public float sprintSpeed;
    public float jumpHeight;
    public float airControl;
    public float groundControl;
    public float gravity;
    public float takeOffSpeed;
    public float glideBuffer;

    public float yLookSensitivity;
    public float xLookSensitivity;
    //

    public InputActionReference move;
    public InputActionReference look;
    public InputActionReference jump;
    public InputActionReference sprint;

    //scaled down

    private Vector3 impliedMoveDir = Vector3.zero;

    private Vector3 velocity = Vector3.zero;//for b hopping and smoothing and whatnot, what actually moves the player

    private Camera cam;

    private float inAirSpeedFallof = 0.5f;
    private (float, float) minMaxPitchLook = (-250f, 250f);
    private float targetPlayerHeight; //this is for making things look smooth
    private float internalGlideBuffer;



    //states

    public enum PlrState
    {
        Sprinting,
        OnGround,
        FreeFall,
        Tumble,
    }
    public PlrState plrState = PlrState.OnGround;
    //

    //camera stuff
    private float pitch = 0;
    private float yaw = 0;


    //
    private float Clamp(float num, float maxNum, float minNum)
    {
        if (num > maxNum) return maxNum;
        else if (num < minNum) return minNum;
        return num;
    }
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = transform.Find("Main Camera").GetComponent<Camera>();

        //state stuff
        jump.action.started += Jump;
    
        
        //
        
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (plrState == PlrState.FreeFall | plrState == PlrState.FreeFall) return;
        internalGlideBuffer = glideBuffer;
        impliedMoveDir = transform.up * jumpHeight;
        velocity = Vector3.Lerp(velocity, impliedMoveDir, 0.1f);
    }

    

    // Update is called once per frame
    void Update()
    {
        //state stuff
        bool clampPitch = true;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.5f,transform.up * -1, out hit, transform.localScale.y + 0.1f) && plrState != PlrState.Tumble)
        {
            if (sprint.action.ReadValue<float>() != 0f)
            {
                plrState = PlrState.Sprinting;
            }
            else
            {
                plrState = PlrState.OnGround;   
            }
            velocity = new Vector3(velocity.x, Clamp(velocity.y, Mathf.Infinity, 0), velocity.z);
            
        }
        else if (plrState != PlrState.Tumble)
        {
            clampPitch = false;
            plrState = PlrState.FreeFall;
        }
        //Debug.Log(hit.point);
        //
        
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
        }
        impliedMoveDir += transform.forward * move.action.ReadValue<Vector2>().y * ((float)localSpeed) * Time.deltaTime;
        impliedMoveDir += transform.right * move.action.ReadValue<Vector2>().x * ((float)localSpeed) * Time.deltaTime;
        if (plrState == PlrState.FreeFall)
        {
            impliedMoveDir += transform.up * -gravity;
        }

        

        
        velocity = Vector3.Lerp(velocity, impliedMoveDir, control);

        
        

        pitch += look.action.ReadValue<Vector2>().y;
        yaw += look.action.ReadValue<Vector2>().x;

       
        
        


        if (clampPitch) pitch = Clamp(pitch, minMaxPitchLook.Item2, minMaxPitchLook.Item1);
        transform.position = transform.position + velocity;

        if ((plrState != PlrState.FreeFall | plrState!= PlrState.Tumble)  && velocity.x <= takeOffSpeed && internalGlideBuffer <1f)
        {
            targetPlayerHeight = hit.point.y + 1f;//ground collision
            Debug.Log("youre on the ground");
        }
        else (targetPlayerHeight) = transform.position.y;
        float distanceMulti = Clamp(1f - (1f / math.abs(targetPlayerHeight - transform.position.y)), 1f, 0.1f);
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetPlayerHeight, transform.position.z), distanceMulti);

        
        cam.transform.localRotation = quaternion.Euler(pitch * -0.01f * yLookSensitivity, 0, 0);
        transform.rotation = quaternion.Euler(0, yaw * 0.01f * xLookSensitivity, 0);

        glideBuffer -= Time.deltaTime;

       
    }
}
