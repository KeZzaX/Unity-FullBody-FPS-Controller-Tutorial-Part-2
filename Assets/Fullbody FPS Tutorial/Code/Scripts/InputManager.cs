using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region Variables
    [Header("Movement Axis")]
    [SerializeField]
    private string m_forwardAxis = "Vertical";
    [SerializeField]
    private string m_sidewayAxis = "Horizontal";

    [Header("Weapon Keys")]
    private KeyCode m_aimKey = KeyCode.Mouse1;

    [Header("Camera Axis")]
    private string m_verticalLookAxis = "Mouse Y";
    private string m_horizontalLookAxis = "Mouse X";
    private float m_xAxisSensitivity = 0.2f;
    private float m_yAxisSensitivity = 0.2f;

    protected float m_forward;
    protected float m_sideway;
    protected bool m_aiming;
    protected float m_xAxis;
    protected float m_yAxis;
    #endregion

    #region Properties
    public float Forward
    {
        get { return m_forward; }
    }

    public float Sideway
    {
        get { return m_sideway; }
    }

    public bool Aiming
    {
        get { return m_aiming; }
    }

    public float XLookAxis
    {
        get { return m_xAxis; }
    }

    public float YLookAxis
    {
        get { return m_yAxis; }
    }
    #endregion

    #region BuiltIn Methods
    private void Update()
    {
        HandleInput();
    }
    #endregion

    #region Custom Methods
    protected void HandleInput()
    {
        m_forward = Input.GetAxis(m_forwardAxis);
        m_sideway = Input.GetAxis(m_sidewayAxis);
        m_aiming = Input.GetKey(m_aimKey);
        m_xAxis = Input.GetAxis(m_horizontalLookAxis) * m_xAxisSensitivity;
        m_yAxis = Input.GetAxis(m_verticalLookAxis) * m_yAxisSensitivity;
    }
    #endregion
}

