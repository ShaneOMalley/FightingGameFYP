using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float KnockbackSpeedToDst(float speed, float dec)
    {
        int num_steps = (int)(speed / dec) + 1;
        return speed * num_steps - (dec * (((num_steps - 1) * num_steps) / 2));
    }

    public static float KnockbackDstToSpeed(float dst, float dec)
    {
        //float speed = 0;
        //float current_distance = 0;

        //while (current_distance <= dst)
        //{
        //    speed += dec;
        //    current_distance += speed;
        //}

        // TODO: Figure out how to do this
        return 69420;
    }
}
