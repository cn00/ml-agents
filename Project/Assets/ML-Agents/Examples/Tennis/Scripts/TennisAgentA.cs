using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;

public class TennisAgentA : Agent
{
    [Header("Specific to Tennis")]
    public GameObject ball;
    public bool invertX;
    // public int score;
    public GameObject myArea;
    public float angle;
    public float scale;

    public Text m_TextComponent;
    public Rigidbody m_AgentRb;
    public Rigidbody m_BallRb;
    public BoxCollider m_Playground;
    public float m_InvertMult;
    public EnvironmentParameters m_ResetParams;

    // Looks for the scoreboard based on the name of the gameObjects.
    // Do not modify the names of the Score GameObjects
    const string k_CanvasName = "Canvas";
    const string k_ScoreBoardAName = "ScoreA";
    const string k_ScoreBoardBName = "ScoreB";
    const float k_velocityMax = 30f;

    [Header("i_1:p_3:r_4:v_3:l_1:lbr_4:bp_3")]
    public List<float> Observations;
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_BallRb = ball.GetComponent<Rigidbody>();
        var canvas = GameObject.Find(k_CanvasName);
        GameObject scoreBoard;
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        if (invertX)
        {
            scoreBoard = canvas.transform.Find(k_ScoreBoardBName).gameObject;
        }
        else
        {
            scoreBoard = canvas.transform.Find(k_ScoreBoardAName).gameObject;
        }
        m_TextComponent = scoreBoard.GetComponent<Text>();
        SetResetParameters();
    }

    /// <summary>
    /// <br/>为了使代理学习，观察应包括代理完成其任务所需的所有信息。如果没有足够的相关信息，座席可能会学得不好或根本不会学。
    /// <br/>确定应包含哪些信息的合理方法是考虑计算该问题的分析解决方案所需的条件，或者您希望人类能够用来解决该问题的方法。<br/>
    /// <br/>产生观察
    /// <br/>   ML-Agents为代理提供多种观察方式：
    /// <br/>
    /// <br/>重写Agent.CollectObservations()方法并将观测值传递到提供的VectorSensor。
    /// <br/>将[Observable]属性添加到代理上的字段和属性。
    /// <br/>ISensor使用SensorComponent代理的附件创建来实现接口ISensor。
    /// <br/>Agent.CollectObservations（）
    /// <br/>Agent.CollectObservations（）最适合用于数字和非可视环境。Policy类调用CollectObservations(VectorSensor sensor)每个Agent的 方法。
    /// <br/>此函数的实现必须调用VectorSensor.AddObservation添加矢量观测值。
    /// <br/>该VectorSensor.AddObservation方法提供了许多重载，可将常见类型的数据添加到观察向量中。
    /// <br/>您可以直接添加整数和布尔值，以观测向量，以及一些常见的统一数据类型，如Vector2，Vector3和Quaternion。
    /// <br/>有关各种状态观察功能的示例，您可以查看ML-Agents SDK中包含的 示例环境。例如，3DBall示例使用平台的旋转，球的相对位置和球的速度作为状态观察。
    /// </summary>
    /// <param name="sensor" type="VectorSensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_InvertMult);               // 角色 x1
        sensor.AddObservation(transform.localPosition);    // 位置 x3

        sensor.AddObservation(transform.localRotation);         // 角度 x4

        sensor.AddObservation(m_AgentRb.velocity);         // 速度 x3
        sensor.AddObservation((transform.localPosition - ball.transform.localPosition).magnitude); // 距离 x1

        sensor.AddObservation(Quaternion.FromToRotation(transform.localPosition, ball.transform.localPosition)); // 相对角度 x4

        // 球的位置和速度
        sensor.AddObservation(ball.transform.localPosition);  // 球位置 x3
        // sensor.AddObservation(ball.transform.localRotation ); // 球角度 x4
        // sensor.AddObservation(m_BallRb.velocity);             // 球速 x3

        Observations = GetObservations().ToList();
    }

    [Header("v_3:r_4")]
    public List<float> m_Actions;

    private static int actionCount = 0;
    /**
     * 动作是代理执行的来自策略的指令。当学院调用代理的OnActionReceived()功能时，该操作将作为参数传递给代理。
     * 代理的动作可以采用两种形式之一，即Continuous或Discrete。
     * 当您指定矢量操作空间为Continuous时，传递给Agent的action参数是长度等于该Vector Action Space Size属性的浮点数数组。
     * 当您指定 离散向量动作空间类型时，动作参数是一个包含整数的数组。每个整数都是命令列表或命令表的索引。
     * 在离散向量操作空间类型中，操作参数是索引数组。数组中的索引数由Branches Size属性中定义的分支数确定。
     * 每个分支对应一个动作表，您可以通过修改Branches 属性来指定每个表的大小。
     * 策略和训练算法都不了解动作值本身的含义。训练算法只是为动作列表尝试不同的值，并观察随着时间的推移和许多训练事件对累积奖励的影响。
     * 因此，仅在OnActionReceived()功能中为代理定义了放置动作。
     * 例如，如果您设计了一个可以在两个维度上移动的代理，则可以使用连续或离散矢量动作。
     * 在连续的情况下，您可以将矢量操作大小设置为两个（每个维一个），并且座席的策略将创建一个具有两个浮点值的操作。
     * 在离散情况下，您将使用一个分支，其大小为四个（每个方向一个），并且策略将创建一个包含单个元素的操作数组，其值的范围为零到三。
     * 或者，您可以创建两个大小为2的分支（一个用于水平移动，一个用于垂直移动），并且Policy将创建一个包含两个元素的操作数组，其值的范围从零到一。
     * 请注意，在为代理编程动作时，使用代理Heuristic()方法测试动作逻辑通常会很有帮助，该方法可让您将键盘命令映射到动作。
     */
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        m_Actions = continuousActions.ToList();
        if (++actionCount % 10000 == 0)
        {
            var s = string.Join(", ", m_Actions);
            Debug.LogWarning($"action_{actionCount}:[{s}] invertX:{invertX} score:{score}");
        }

        int i = 0;
        var velocityX   = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var velocityY   = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var velocityZ   = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var rotateX     = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var rotateY     = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var rotateZ     = Mathf.Clamp(continuousActions[i++], -1f, 1f);
        var rotateW     = Mathf.Clamp(continuousActions[i++], -1f, 1f);

        m_AgentRb.velocity = new Vector3(velocityX * k_velocityMax, velocityY * k_velocityMax, velocityZ * k_velocityMax);

        m_AgentRb.transform.localRotation = new Quaternion(rotateX, rotateY, rotateZ, rotateW); // Quaternion.Euler(90f * rotateX, 90f * rotateY, 90f * rotateZ);
        // m_AgentRb.transform.localEulerAngles = new Vector3(180f*rotateX, 180f*rotateY, 180f*rotateZ); // Quaternion.Euler(90f * rotateX, 90f * rotateY, 90f * rotateZ);

        // var rect = new Rect(m_Playground.center, m_Playground.size);
        // if (!rect.Contains(transform.localPosition))
        // {
        //     transform.localPosition = new Vector3(
        //         -m_InvertMult * 7f,
        //         -7f,
        //         -1.8f);
        // }

        m_TextComponent.text = score.ToString();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");              // moveX Racket Movement
        continuousActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1f : 0f;    // moveY Racket Jumping
        continuousActionsOut[2] = Input.GetAxis("Vertical");                // moveZ
        if(SystemInfo.supportsGyroscope)
        {
            var ang = Input.gyro.attitude.eulerAngles;
            continuousActionsOut[3] = Input.gyro.attitude.x; // rotateX
            continuousActionsOut[4] = Input.gyro.attitude.y; // rotateY
            continuousActionsOut[5] = Input.gyro.attitude.z; // rotateZ
            continuousActionsOut[6] = Input.gyro.attitude.w; // rotateW
        }
    }

    public override void OnEpisodeBegin()
    {
        m_InvertMult = invertX ? -1f : 1f;

        // transform.position = new Vector3(-m_InvertMult * Random.Range(6f, 8f), -1.5f, -1.8f) + transform.parent.transform.position;
        transform.localPosition = new Vector3(
            -m_InvertMult * Random.Range(6f, 8f),
            -1.5f,
            -1.8f);
        m_AgentRb.velocity = new Vector3(0f, 0f, 0f);

        SetResetParameters();
    }

    public void SetRacket()
    {
        angle = m_ResetParams.GetWithDefault("angle", 55);
        gameObject.transform.eulerAngles = new Vector3(
            gameObject.transform.eulerAngles.x,
            gameObject.transform.eulerAngles.y,
            m_InvertMult * angle
        );
    }

    public void SetBall()
    {
        scale = m_ResetParams.GetWithDefault("scale", .5f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        SetRacket();
        SetBall();
    }
}
