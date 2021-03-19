using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [HideInInspector]
    public float tacticWight;    
    [HideInInspector]
    public float quality;
    [HideInInspector]
    public float cover = 0f;
    [HideInInspector]
    public float visibility = 0f;
    [HideInInspector]
    public float distanceFromBase = 0f;
    [HideInInspector]
    public float ourUnitsNearby = 0f;
    [HideInInspector]
    public bool canSeeEnemy = false;

    float wight_distance = 0.6f/200;
    float wight_cover = 0.1f/8;
    float wight_visibility = 0.1f/30;
    float wight_dfb = 0.1f/200;
    float tacticWight_inSight = 0.1f/600;


    public float GetTacticWightValue(float d = 0f)
    {
        float c = 0f;
        c = d * wight_distance + (8 - cover) * wight_cover + visibility * wight_visibility + distanceFromBase * wight_dfb + ourUnitsNearby * tacticWight_inSight;

        return c;
    }

    public float GetQualityValue()
    {
        return (8 - cover) * wight_cover + visibility * wight_visibility + distanceFromBase * wight_dfb;
    }

    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < 8; i++)
    //    {
    //        Vector3 d = Quaternion.AngleAxis(0 + 45 * i, Vector3.up) * transform.forward;
    //        Gizmos.DrawRay(transform.position, d * 10);
    //    }
    //}


    public int checkCollision()
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3 d = Quaternion.AngleAxis(0 + 45 * i, Vector3.up) * transform.forward;

            if (Physics.Raycast(transform.position, d, 10f))
            {
                count++;
            }
        }
        return count;
    }


    public void toString()
    {
        Debug.Log( this.ToString() +
            "[ cover=" + cover +
            ", visibility=" + visibility+
            ", distanceFromBase="+ distanceFromBase+
            ", ourUnitsNearby="+ ourUnitsNearby+
            "]-------TacticWightValue = "+ tacticWight);
    }

}
