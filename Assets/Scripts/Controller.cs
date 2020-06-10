using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; protected set; }

    public Camera MainCamera;
    public Transform CameraPosition;
    
    [Header("Control Settings")]
    public float MouseSensitivity = 5.0f;
    public float PlayerSpeed = 10.0f;
    public float JumpSpeed = 5.0f;

    [Header("Camera Position")]
    public float PositionY = 2.0f;
    public float PositionZ = -5.0f;
    
    float m_VerticalSpeed = 0.0f;
    bool m_IsPaused = false;
    
    float m_VerticalAngle, m_HorizontalAngle;
    public float Speed { get; private set; } = 0.0f;

    public bool LockControl { get; set; }
    public bool CanPause { get; set; } = true;

    public bool Grounded => m_Grounded;

    CharacterController m_CharacterController;

    bool m_Grounded;
    float m_GroundedTimer;
    float m_SpeedAtJump = 0.0f;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_IsPaused = false;
        m_Grounded = true;
        
        MainCamera.transform.SetParent(CameraPosition, false);
        MainCamera.transform.localPosition = new Vector3(0, PositionY, PositionZ);
        MainCamera.transform.localRotation = Quaternion.identity;
        m_CharacterController = GetComponent<CharacterController>();

        m_VerticalAngle = 0.0f;
        m_HorizontalAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        bool wasGrounded = m_Grounded;
        bool loosedGrounding = false;
        
        if (!m_CharacterController.isGrounded)
        {
            if (m_Grounded)
            {
                m_GroundedTimer += Time.deltaTime;
                if (m_GroundedTimer >= 0.5f)
                {
                    loosedGrounding = true;
                    m_Grounded = false;
                }
            }
        }
        else
        {
            m_GroundedTimer = 0.0f;
            m_Grounded = true;
        }

        Speed = 0;
        Vector3 move = Vector3.zero;
        if (!m_IsPaused && !LockControl)
        {
            if (m_Grounded && Input.GetButtonDown("Jump"))
            {
                m_VerticalSpeed = JumpSpeed;
                m_Grounded = false;
                loosedGrounding = true;
            }
            
            float actualSpeed = PlayerSpeed;

            if (loosedGrounding)
            {
                m_SpeedAtJump = actualSpeed;
            }

            move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude > 1.0f)
                move.Normalize();

            float usedSpeed = m_Grounded ? actualSpeed : m_SpeedAtJump;
            
            move = move * usedSpeed * Time.deltaTime;
            
            move = transform.TransformDirection(move);
            m_CharacterController.Move(move);
            
            float turnPlayer =  Input.GetAxis("Mouse X") * MouseSensitivity;
            m_HorizontalAngle = m_HorizontalAngle + turnPlayer;

            if (m_HorizontalAngle > 360) m_HorizontalAngle -= 360.0f;
            if (m_HorizontalAngle < 0) m_HorizontalAngle += 360.0f;
            
            Vector3 currentAngles = transform.localEulerAngles;
            currentAngles.y = m_HorizontalAngle;
            transform.localEulerAngles = currentAngles;

            var turnCam = -Input.GetAxis("Mouse Y");
            turnCam = turnCam * MouseSensitivity;
            m_VerticalAngle = Mathf.Clamp(turnCam + m_VerticalAngle, -89.0f, 89.0f);
            currentAngles = CameraPosition.transform.localEulerAngles;
            currentAngles.x = m_VerticalAngle;
            CameraPosition.transform.localEulerAngles = currentAngles;
  
            Speed = move.magnitude / (PlayerSpeed * Time.deltaTime);
        }

        m_VerticalSpeed = m_VerticalSpeed - 10.0f * Time.deltaTime;
        if (m_VerticalSpeed < -10.0f)
            m_VerticalSpeed = -10.0f; // max fall speed
        var verticalMove = new Vector3(0, m_VerticalSpeed * Time.deltaTime, 0);
        var flag = m_CharacterController.Move(verticalMove);
        if ((flag & CollisionFlags.Below) != 0)
            m_VerticalSpeed = 0;
    }
}
