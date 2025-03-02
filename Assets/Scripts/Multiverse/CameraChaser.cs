using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChaser : MonoBehaviour
{
    GameObject player;

    private Vector3? chasingPos = null;
    Vector3 startPos;
    bool zoomingIn = false;
    float progress;

    public void SetGoal(Vector3 start, Vector3? goal, bool zoomingIn)
    {
        progress = 0;
        startPos = start;
        transform.position = startPos;
        this.zoomingIn = zoomingIn;
        chasingPos = goal;

        if (zoomingIn)
            transform.position = new Vector3(transform.position.x, transform.position.y, 17.5f);
        else
            transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
    }

    Vector3 GoalPos()
    {
        if (chasingPos == null)
        {
            if (zoomingIn)
            {
                return new Vector3(player.transform.position.x, player.transform.position.y, 1);
            } else
            {
                return new Vector3(player.transform.position.x, player.transform.position.y, -17.5f);
            }
        } else
        {
            return chasingPos.Value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
        player = GameObject.Find("Player");

        SetGoal(GameSettings.multiverseStartPoint, null, false);
    }

    public static float MapRange(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    private float progressVelocity = 0.0f; // Velocity for SmoothDamp

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = player.transform.rotation;

        progress = Mathf.SmoothDamp(progress, 1f, ref progressVelocity, 0.5f); // Smoothly transition progress to 1

        transform.position = Vector3.Lerp(startPos, GoalPos(), progress);

        ScreenDissolveFeature.Instance.progress = MapRange(transform.position.z, 1, -17.5f, 0, 1);
        //if (reached)
        //{
        //    transform.position = GoalPos();
        //} else
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, GoalPos(), 5f * Time.deltaTime);
        //}
        //if (zoomingIn)
        //{
        //    transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.MoveTowards(transform.position.z, 1f, 5f * Time.deltaTime));
        //} else
        //{
        //    transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.MoveTowards(transform.position.z, -17.5f, 5f * Time.deltaTime));
        //}
        //ScreenDissolveFeature.Instance.progress = MapRange(transform.position.z, 1, -17.5f, 0, 1);
    }
}
