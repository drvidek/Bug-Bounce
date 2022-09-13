using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum RotAxis { hor, ver }
    [SerializeField] private RotAxis rotAxis = RotAxis.hor;
    [SerializeField] private Joystick _stickCam;
    [SerializeField] private float _sensitivity = 40f;
    [SerializeField] private float _verticalClamp = 30f;
    private float _verRot;
    [SerializeField] private bool _inverted;

    private void Start()
    {
        _verRot = transform.localEulerAngles.x;
    }

    Vector3 GetInputDirection()
    {
        Vector3 dir = new Vector3();
        dir.x = _stickCam.Horizontal;
        if (dir.x == 0)
            dir.x = Input.GetAxis("CamHorizontal");
        dir.z = _stickCam.Vertical;
        if (dir.z == 0)
            dir.z = Input.GetAxis("CamVertical");
        return dir;
    }


    private void Update()
    {
        Vector3 camDir = GetInputDirection();
        if (rotAxis == RotAxis.ver)
        {
            _verRot += camDir.z * _sensitivity * Time.deltaTime * (_inverted ? -1 : 1);
            _verRot = Mathf.Clamp(_verRot, -_verticalClamp, _verticalClamp);
            transform.localEulerAngles = new Vector3(_verRot, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        else
        {
            float _horRot = camDir.x * _sensitivity * Time.deltaTime * (_inverted ? -1 : 1);
            transform.Rotate(0, _horRot, 0);
        }
    }
}
