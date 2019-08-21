using UnityEngine;
using RootMotion.FinalIK;
public class UpperBodyIK : MonoBehaviour
{
    #region Variables
    [Header("KZX Modules")]
    [SerializeField]
    private InputManager m_inputManager = default;

    [Header("Final IK Modules")]
    [SerializeField]
    private LookAtIK m_headLookAtIK = default;
    [SerializeField]
    private LookAtIK m_bodyLookAtIK = default;
    [SerializeField]
    private ArmIK m_leftArmIK = default;
    [SerializeField]
    private ArmIK m_rightArmIK = default;
    [SerializeField]
    private FullBodyBipedIK m_fbbIK = default;

    [Header("LookAt Settings")]
    [SerializeField]
    private Transform m_camera = default;
    [SerializeField]
    private Transform m_headTarget = default;
    [SerializeField]
    private Transform m_bodyTarget = default;
    [Range(-89, 0)]
    [SerializeField]
    private float _maxAngleUp = 65f;
    [Range(0, 89)]
    [SerializeField]
    private float _maxAngleDown = 80f;
    [Range(-89f, 89f)]
    [SerializeField]
    private float m_bodyOffsetAngle = 45f;
    [SerializeField]
    private float m_rotateSpeed = 7f;


    [Header("Head Effector Settings")]
    [SerializeField]
    private Transform m_headEffector = default;
    [SerializeField]
    private Transform m_headEffectorNeutral = default;
    [SerializeField]
    private Transform m_headEffectorUp = default;
    [SerializeField]
    private Transform m_headEffectorDown = default;

    [Header("Arms Settings")]
    [SerializeField]
    private Transform m_rightHandTarget = default;
    [SerializeField]
    private float m_rightHandPosSpeed = 1f;
    [SerializeField]
    private float m_rightHandRotSpeed = 1f;

    [Header("ADS Settings")]
    [SerializeField]
    private Transform m_rightHandHips = default;
    [SerializeField]
    private Transform m_rightHandADS = default;
    [SerializeField]
    private float m_adsTransitionTime = 1f;
    [SerializeField]
    private Camera m_mainCamera = default;
    [SerializeField]
    private float m_hipsFov = 60f;
    [SerializeField]
    private float m_adsFov = 40f;

    [Header("Sway settings")]
    [SerializeField]
    private float m_A = 1;
    [SerializeField]
    private float m_B = 2;
    [SerializeField]
    private float m_sizeReducerFactor = 10f;
    [SerializeField]
    private float m_thetaIncreaseFactor = 0.01f;
    [SerializeField]
    private float m_swayLerpSpeed = 15f;

    private float m_transitionADS;
    private Vector3 m_rightHandFollow;
    private Quaternion m_rightHandFollowRot;
    private Vector3 m_refRightHandFollow;
    private float m_theta;
    private Vector3 m_swayPos;
    private float m_currentBodyAngle;

    #endregion

    #region BuiltIn Methods
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_headLookAtIK.enabled = false;
        m_bodyLookAtIK.enabled = false;
        m_rightArmIK.enabled = false;
        m_leftArmIK.enabled = false;
        m_fbbIK.enabled = false;

        m_currentBodyAngle = m_bodyOffsetAngle;
    }

    private void Update()
    {
        m_bodyLookAtIK.solver.FixTransforms();
        m_headLookAtIK.solver.FixTransforms();
        m_fbbIK.solver.FixTransforms();
        m_rightArmIK.solver.FixTransforms();
        m_leftArmIK.solver.FixTransforms();
    }

    private void LateUpdate()
    {
        LookAtIKUpdate();
        FBBIKUpdate();
        ArmsIKUpdate();
    }
    #endregion

    #region Custom Methods
    private void LookAtIKUpdate()
    {
        m_bodyLookAtIK.solver.Update();
        m_headLookAtIK.solver.Update();
    }

    private void ArmsIKUpdate()
    {
        UpdateSwayOffset();
        AimDownSightUpdate();
        m_rightArmIK.solver.Update();
        m_leftArmIK.solver.Update();
    }

    private void AimDownSightUpdate()
    {
        if (m_inputManager.Aiming == false)
        {
            m_transitionADS = Mathf.Lerp(m_transitionADS, 0, Time.smoothDeltaTime * m_adsTransitionTime);
            m_rightHandTarget.rotation = m_rightHandHips.rotation;
        }
        else
        {
            m_transitionADS = Mathf.Lerp(m_transitionADS, 1, Time.smoothDeltaTime * m_adsTransitionTime);
            m_rightHandTarget.rotation = m_rightHandADS.rotation;

        }

        m_rightHandFollow = Vector3.Lerp(m_rightHandHips.position, m_rightHandADS.position, m_transitionADS);
        m_rightHandFollowRot = Quaternion.Lerp(m_rightHandHips.rotation, m_rightHandADS.rotation, m_transitionADS);
        m_mainCamera.fieldOfView = Mathf.Lerp(m_hipsFov, m_adsFov, m_transitionADS);

        m_rightHandFollow += m_camera.TransformVector(m_swayPos);

        m_rightHandTarget.position = Vector3.SmoothDamp(m_rightHandTarget.position, m_rightHandFollow, ref m_refRightHandFollow, m_rightHandPosSpeed * Time.smoothDeltaTime);
        m_rightHandTarget.rotation = Quaternion.Lerp(m_rightHandTarget.rotation, m_rightHandFollowRot, Time.smoothDeltaTime * m_rightHandRotSpeed);
    }

    private void FBBIKUpdate()
    {
        m_fbbIK.solver.Update();

        m_camera.LookAt(m_headTarget);
        m_headEffector.LookAt(m_headTarget);

        UpdateLookTargetPos();
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(m_camera.transform.forward.x, 0f, m_camera.transform.forward.z)), Time.smoothDeltaTime * m_rotateSpeed);
    }

    private void UpdateLookTargetPos()
    {
        Vector3 targetForward = Quaternion.LookRotation(new Vector3(m_camera.transform.forward.x, 0f, m_camera.transform.forward.z)) * Vector3.forward;
        float angle = Vector3.SignedAngle(targetForward, m_camera.forward, m_camera.right);

        float percent;
        float maxY = 100f;
        float minY = -100f;
        if (angle < 0)
        {
            percent = Mathf.Clamp01(angle / _maxAngleUp);
            if (percent >= 1f)
            {
                maxY = 0f;
            }
            m_headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position, m_headEffectorUp.position, percent);
        }
        else
        {
            percent = Mathf.Clamp01(angle / _maxAngleDown);
            if (percent >= 1f)
            {
                minY = 0f;
            }
            m_headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position, m_headEffectorDown.position, percent);
        }
        m_currentBodyAngle = Mathf.Lerp(m_bodyOffsetAngle, 0, percent);
 

        Vector3 offset = m_camera.right * m_inputManager.XLookAxis + m_camera.up * Mathf.Clamp(m_inputManager.YLookAxis, minY, maxY);
        offset += m_headTarget.transform.position;
        Vector3 projectedPoint = (offset - m_camera.position).normalized * 20f + m_camera.position;

        m_headTarget.transform.position = projectedPoint;
        m_bodyTarget.transform.position = GetPosFromAngle(projectedPoint, m_currentBodyAngle, transform.right);
    }

    private Vector3 GetPosFromAngle(Vector3 projectedPoint, float angle, Vector3 axis)
    {
        float dist = (projectedPoint - transform.position).magnitude * Mathf.Tan(angle * Mathf.Deg2Rad);
        return projectedPoint + (dist * axis);
    }

    private void UpdateSwayOffset()
    {
        Vector3 targetPos = (LissajousCurve(m_theta, m_A, Mathf.PI, m_B) / m_sizeReducerFactor);
        m_swayPos = Vector3.Lerp(m_swayPos, targetPos, Time.smoothDeltaTime * m_swayLerpSpeed);
        m_theta += m_thetaIncreaseFactor;
    }

    private Vector3 LissajousCurve(float theta, float A, float delta, float B)
    {
        Vector3 pos = Vector3.zero;
        pos.x = Mathf.Sin(theta);
        pos.y = A * Mathf.Sin(B * theta + delta);
        return pos;
    }
    #endregion
}

