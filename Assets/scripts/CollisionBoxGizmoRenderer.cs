using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBoxGizmoRenderer : MonoBehaviour
{
    private static Sprite BoxSprite;
    private const string SPRITE_NAME = "boxSprite";

    private GameObject _spritesGo;

    void Start()
    {
        // Intialize the resource if it hasn't been done yet
        if (BoxSprite == null)
        {
            BoxSprite = Resources.Load<Sprite>(SPRITE_NAME);
        }

        foreach (Transform boxGroup in transform)
        {
            // Set up color of sprite GameObject
            Color color;
            float relZ;
            switch (boxGroup.name)
            {
                case "hurt":
                    color = Color.green;
                    relZ = -0.2f;
                    break;
                case "hit":
                    color = Color.red;
                    relZ = -0.3f;
                    break;
                case "push":
                    color = Color.magenta;
                    relZ = -0.1f;
                    break;
                case "throw":
                    color = Color.blue;
                    relZ = -0.35f;
                    break;
                case "prox":
                    color = Color.yellow;
                    relZ = -0.15f;
                    break;
                case "camera":
                    color = new Color(0, 0, 0, 0);
                    relZ = 0.0f;
                    break;
                default:
                    color = Color.white;
                    relZ = -2f;
                    break;
            }

            // Set up GameObject to hold sprite-rendering GameObjects
            GameObject spritesGo = new GameObject("sprites");
            spritesGo.transform.Translate(0, 0, relZ);
            spritesGo.transform.SetParent(boxGroup);
            
            foreach (BoxCollider2D box in boxGroup.GetComponents<BoxCollider2D>())
            {
                // Setup GameObject to render sprite
                GameObject spriteGo = new GameObject("sprite");
                spriteGo.transform.SetParent(spritesGo.transform);
                SpriteRenderer sr = spriteGo.AddComponent<SpriteRenderer>();
                sr.sprite = BoxSprite;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.color = color;
                sr.enabled = false;

                // Set position and dimensions of sprite
                spriteGo.transform.localPosition = box.bounds.min;
                float w = box.bounds.size.x;
                float h = box.bounds.size.y;
                sr.size = new Vector2(w, h);
            }
        }
    }

    private void setSpriteRenderersActiveRecursively(bool enabled)
    {
        foreach (Transform boxGroup in transform)
        {
            Transform sprites = boxGroup.Find("sprites");
            foreach (Transform sprite in sprites)
            {
                sprite.GetComponent<SpriteRenderer>().enabled = enabled;
            }
        }
    }

    private void Update()
    {
        setSpriteRenderersActiveRecursively(Globals.DRAW_HITBOXES);
    }
}
