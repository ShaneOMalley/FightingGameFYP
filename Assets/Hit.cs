﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour
{
    public void Destroy()
    {
        Debug.Log("Destroying");
        Destroy(gameObject);
    }
}
