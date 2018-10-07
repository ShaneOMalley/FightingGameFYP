using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform[] Subjects;
    public Transform CameraBoundsLeft;
    public Transform CameraBoundsRight;

    public float Padding = 0.3f;
    public float MinCameraWidth = 16;
    public float MaxCameraWidth = 16;

    private Camera _camera;
    private float _gap_percentage;

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
            return transform.position.x - CameraWidth / 2;
        }
    }

    public float Right
    {
        get
        {
            return transform.position.x + CameraWidth / 2;
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
    }
	
	void Update ()
    {
        Vector3 target_pos = getAveragePos2d(Subjects);
        transform.position = new Vector3(target_pos.x, transform.position.y, transform.position.z);

        // Clamp camera position between bounds
        float left = transform.position.x - CameraWidth / 2;
        float right = transform.position.x + CameraWidth / 2;

        if (left < CameraBoundsLeft.position.x)
        {
            transform.position = new Vector3(CameraBoundsLeft.position.x + CameraWidth / 2, transform.position.y, transform.position.z);
        }
        else if (right > CameraBoundsRight.position.x)
        {
            transform.position = new Vector3(CameraBoundsRight.position.x - CameraWidth / 2, transform.position.y, transform.position.z);
        }

        // Control Camera Zoom
        float leftmostSubjectX = Subjects.Min(subject => subject.position.x);
        float rightMostSubjectX = Subjects.Max(subject => subject.position.x);

        float distance = rightMostSubjectX - leftmostSubjectX;
        CameraWidth = distance * (1 / _gap_percentage);
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