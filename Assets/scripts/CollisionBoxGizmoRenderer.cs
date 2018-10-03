using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBoxGizmoRenderer : MonoBehaviour
{
    public enum Kind { HURT, HIT, PUSH, THROW, PROX }
    public Kind kind = Kind.HURT;

	void Start()
    {
	}

    private void Update()
    {
        foreach (Transform boxGroup in transform)
        {
            Color color;
            switch (boxGroup.name)
            {
                case "hurt":
                    color = Color.green;
                    break;
                case "hit":
                    color = Color.red;
                    break;
                case "push":
                    color = Color.magenta;
                    break;
                case "throw":
                    color = Color.blue;
                    break;
                case "prox":
                    color = Color.yellow;
                    break;
                case "camera":
                    color = Color.black;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            foreach (BoxCollider2D box in boxGroup.GetComponents<BoxCollider2D>())
            {
                float left = box.bounds.min.x;
                float right = box.bounds.max.x;
                float bottom = box.bounds.min.y;
                float top = box.bounds.max.y;
                float z = box.bounds.min.z;

                Vector3 topLeft = new Vector3(left, top, z);
                Vector3 bottomRight = new Vector3(right, bottom, z);
                Vector3 bottomLeft = box.bounds.min;
                Vector3 topRight = box.bounds.max;
                
                Debug.DrawLine(topLeft, topRight, color);
                Debug.DrawLine(bottomLeft, bottomRight, color);
                Debug.DrawLine(topRight, bottomRight, color);

                float step = (right - left) / 10f;

                for (float x = left; x < right; x+=step)
                {
                    Debug.DrawLine(new Vector3(x, bottom, z), new Vector3(x, top, z), color);
                }
            }
        }
    }
}
