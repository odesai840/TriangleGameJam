using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraChaser : MonoBehaviour
{
    GameObject player;

    public Vector3? chasingPos = null;
    public Vector3 startPos;
    public bool zoomingIn = false;
    public float progress;

    public int universeType;

    public void SetGoal(Vector3 start, Vector3? goal, bool zoomingIn)
    {
        progress = 0;
        startPos = start;
        transform.position = startPos;
        this.zoomingIn = zoomingIn;
        chasingPos = goal;

        if (zoomingIn)
            transform.position = new Vector3(transform.position.x, transform.position.y, -17.5f);
        else
            transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
        startPos = transform.position;
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
            if (zoomingIn)
            {
                return new Vector3(chasingPos.Value.x, chasingPos.Value.y, 1);
            }
            else
            {
                return new Vector3(chasingPos.Value.x, chasingPos.Value.y, -17.5f);
            }
            //return chasingPos.Value;
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
        if (player != null)
            transform.rotation = player.transform.rotation;

        progress = Mathf.SmoothDamp(progress, 1f, ref progressVelocity, 0.5f); // Smoothly transition progress to 1

        transform.position = Vector3.Lerp(startPos, GoalPos(), progress);

        ScreenDissolveFeature.Instance.progress = MapRange(transform.position.z, 1, -17.5f, 0, 1);

        if (zoomingIn && progress > 0.9f)
        {
            switch (universeType)
            {
                case 0:
                case 1:
                case 2:
                    SceneManager.LoadScene("Type" + (universeType + 1));
                    break;
                case 3:
                    GameSettings.multiverseStartPoint = new Vector3(0, 0, 0);
                    SceneManager.LoadScene("Multiverse");
                    break;
                case 4:
                    SceneManager.LoadScene("Title"); //replace with end animation
                    break;
            }
        }
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
