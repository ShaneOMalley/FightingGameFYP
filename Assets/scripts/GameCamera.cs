using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public GameController GameController;
    //[HideInInspector]
    public Transform[] Subjects;
    public Transform CameraBoundsLeft;
    public Transform CameraBoundsRight;
    
    [HideInInspector]
    public Vector3 _offsetPos;
    private Vector3 _centerPos;

    public float Padding = 0.3f;
    public float MinCameraWidth = 16;
    public float MaxCameraWidth = 16;

    private Camera _camera;
    private float _gap_percentage;

    private Animator _animator;

    private bool CameraMoving
    {
        get
        {
            if (GameController.Data.P1.Holding || GameController.Data.P2.Holding)
                return false;

            return true;
        }
    }

    public float CameraWidth
    {
        get
        {
            return _camera.orthographicSize * _camera.aspect * 2;
        }

        set
        {
            value = Mathf.Clamp(value, MinCameraWidth, MaxCameraWidth);
            _camera.orthographicSize = value / (_camera.aspect * 2);
        }
    }

    public float Left
    {
        get
        {
            return _centerPos.x - CameraWidth / 2;
        }
    }

    public float Right
    {
        get
        {
            return _centerPos.x + CameraWidth / 2;
        }
    }

    void Start ()
    {
        _camera = GetComponent<Camera>();

        // The percentage of the screen which will be taken up by the gap
        _gap_percentage = 1 - (Padding * 2);

        float upper_max_width = CameraBoundsRight.position.x - CameraBoundsLeft.position.x;
        if (MinCameraWidth > upper_max_width) MinCameraWidth = upper_max_width;
        if (MaxCameraWidth > upper_max_width) MaxCameraWidth = upper_max_width;

        _centerPos = transform.position;
        _offsetPos = Vector3.zero;

        _animator = GetComponent<Animator>();
    }
	
	void Update ()
    {
        if (CameraMoving)
        {
            // Apply camera shake
            _animator.SetLayerWeight(1, GameController.InHitFreeze() ? 0.25f : 0);

            Vector3 target_pos = getAveragePos2d(Subjects);
            //transform.position = new Vector3(target_pos.x, transform.position.y, transform.position.z);
            //_centerPos = new Vector3(target_pos.x, _centerPos.y, _centerPos.z);
            _centerPos = new Vector3(Mathf.Lerp(_centerPos.x, target_pos.x, 0.25f), _centerPos.y, _centerPos.z);

            //transform.position = new Vector3(Mathf.Lerp(transform.position.x, target_pos.x, 0.2f), transform.position.y, transform.position.z);

            // Clamp camera position between bounds
            //float left = _centerPos.x - CameraWidth / 2;
            //float right = _centerPos.x + CameraWidth / 2;

            if (Left < CameraBoundsLeft.position.x)
            {
                _centerPos = new Vector3(CameraBoundsLeft.position.x + CameraWidth / 2, _centerPos.y, _centerPos.z);
            }
            else if (Right > CameraBoundsRight.position.x)
            {
                _centerPos = new Vector3(CameraBoundsRight.position.x - CameraWidth / 2, _centerPos.y, _centerPos.z);
            }

            // Update position, allowing for shaking effects etc.
            transform.position = _centerPos + _offsetPos;

            // Control Camera Zoom
            float leftmostSubjectX = Subjects.Min(subject => subject.position.x);
            float rightMostSubjectX = Subjects.Max(subject => subject.position.x);

            float distance = rightMostSubjectX - leftmostSubjectX;
            CameraWidth = distance * (1 / _gap_percentage);
        }
    }

    private Vector3 getAveragePos2d(Transform[] transforms)
    {
        Vector3 average_pos = Vector2.zero;
        foreach (Transform subject in Subjects)
        {
            average_pos += subject.position;
        }
        average_pos /= Subjects.Length;
        return average_pos;
    }
}