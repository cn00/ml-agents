using UnityEngine;

public class TennisArea : MonoBehaviour
{
    public GameObject ball;
    public GameObject agentA;
    public GameObject agentB;
    Rigidbody m_BallRb;

    public float minPosX = 6f;
    public float maxPosX = 8f;
    public float minPosY = 5f;
    public float maxPosY = 9f;
    public float minPosZ = -3f;
    public float maxPosZ = 3f;

    // Use this for initialization
    void Start()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        MatchReset();
    }

    public void MatchReset()
    {
        var ballOut = Random.Range(minPosX, maxPosX);
        var flip = Random.Range(0, 2);
        if (flip == 0)
        {
            ball.transform.localPosition = new Vector3(-ballOut, Random.Range(minPosY, maxPosY), Random.Range(minPosZ, maxPosZ));
        }
        else
        {
            ball.transform.localPosition = new Vector3(ballOut, Random.Range(minPosY, maxPosY), Random.Range(minPosZ, maxPosZ));
        }
        m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.localScale = new Vector3(.5f, .5f, .5f);
    }

    void FixedUpdate()
    {
        var rgV = m_BallRb.velocity;
        m_BallRb.velocity = new Vector3(
            Mathf.Clamp(rgV.x, -9f, 9f),
            Mathf.Clamp(rgV.y, -9f, 9f),
            Mathf.Clamp(rgV.z, -9f, 9f));
    }
}
