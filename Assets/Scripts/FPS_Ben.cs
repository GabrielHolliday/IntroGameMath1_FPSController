using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPS_Ben : MonoBehaviour
{
    //public stuff
    public int speed;
    public int jumpHeight;
    public int airControllMulti;

    public float yLookSensitivity;
    public float xLookSensitivity;
    //

    public InputActionReference move;
    public InputActionReference look;
    //scaled down

    private Vector3 impliedMoveDir = Vector3.zero;

    private Vector3 velocity = Vector3.zero;//for b hopping and smoothing and whatnot, what actually moves the player

    private Camera cam;

    private bool onGround = true;
    private bool sprinting = true;

    //camera stuff
    private float pitch = 0;
    private float yaw = 0;
    //
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = transform.Find("Main Camera").GetComponent<Camera>();

        
    }

    private void Jump()
    {

    }

    // Update is called once per frame
    void Update()
    {
        impliedMoveDir = Vector3.zero;
        
        impliedMoveDir = transform.forward * move.action.ReadValue<Vector2>().y * ((float)speed) * Time.deltaTime;
        impliedMoveDir += transform.right * move.action.ReadValue<Vector2>().x  * ((float)speed) * Time.deltaTime;
        
        

        velocity = Vector3.Lerp(velocity, impliedMoveDir, 0.01f);

        pitch += look.action.ReadValue<Vector2>().y;
        yaw += look.action.ReadValue<Vector2>().x;
      
        transform.position = transform.position + velocity;
        cam.transform.localRotation = quaternion.Euler(pitch * -0.01f * yLookSensitivity, 0, 0);
        transform.rotation = quaternion.Euler(0, yaw * 0.01f * xLookSensitivity, 0);
        


       
    }
}
