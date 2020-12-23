using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

public class HitWall : MonoBehaviour
{

    public bool net;

    public enum AgentRole
    {
        A,
        B,
        O,
    }
    public AgentRole lastAgentHit = AgentRole.O;

    public enum FloorHit
    {
        Service,
        FloorHitUnset,
        FloorAHit,
        FloorBHit
    }
    public FloorHit lastFloorHit;

    public TennisArea m_Area;
    public Agent m_AgentA;
    public Agent m_AgentB;

    //  Use this for initialization
    void Start()
    {
        // m_Area = areaObject.GetComponent<TennisArea>();
        // m_AgentA = m_Area.agentA.GetComponent<TennisAgentA>();
        // m_AgentB = m_Area.agentB.GetComponent<TennisAgentA>();
    }

    void Reset()
    {
        m_AgentA.EndEpisode();
        m_AgentB.EndEpisode();
        m_Area.MatchReset();
        lastFloorHit = FloorHit.Service;
        lastAgentHit = AgentRole.O;
        net = false;
    }

    void AgentAWins()
    {
        m_AgentA.SetReward(1);
        m_AgentB.SetReward(-1);
        m_AgentA.score += 1;
        Reset();

    }

    void AgentBWins()
    {
        m_AgentA.SetReward(-1);
        m_AgentB.SetReward(1);
        m_AgentB.score += 1;
        Reset();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("iWall"))
        {
            if (collision.gameObject.name == "wallA")
            {
                // Agent A hits into wall or agent B hit a winner
                if (lastAgentHit == AgentRole.A || lastFloorHit == FloorHit.FloorAHit)
                {
                    AgentBWins();
                }
                // Agent B hits long
                else
                {
                    AgentAWins();
                }
            }
            else if (collision.gameObject.name == "wallB")
            {
                // Agent B hits into wall or agent A hit a winner
                if (lastAgentHit == AgentRole.B || lastFloorHit == FloorHit.FloorBHit)
                {
                    AgentAWins();
                }
                // Agent A hits long
                else
                {
                    AgentBWins();
                }
            }
            else if (collision.gameObject.name == "floorA")
            {
                // Agent A hits into floor, double bounce or service
                if (   lastAgentHit == AgentRole.A
                    || lastFloorHit == FloorHit.FloorAHit
                    || lastFloorHit == FloorHit.Service)
                {
                    AgentBWins();
                }
                else
                {
                    lastFloorHit = FloorHit.FloorAHit;
                    //successful serve 过网？
                    if (lastAgentHit == AgentRole.B && !net)
                    {
                        net = true;
                    }
                }
            }
            else if (collision.gameObject.name == "floorB")
            {
                // Agent B hits into floor, double bounce or service
                if (lastAgentHit == AgentRole.B || lastFloorHit == FloorHit.FloorBHit || lastFloorHit == FloorHit.Service)
                {
                    AgentAWins();
                }
                else
                {
                    lastFloorHit = FloorHit.FloorBHit;
                    //successful serve
                    if (lastAgentHit == AgentRole.A && !net)
                    {
                        net = true;
                    }
                }
            }
            else if (collision.gameObject.name == "net" && !net)
            {
                if (lastAgentHit == AgentRole.A)
                {
                    AgentBWins();
                }
                else if (lastAgentHit == AgentRole.B)
                {
                    AgentAWins();
                }
            }
        }
        else if (collision.gameObject.name == "AgentA")
        {
            m_AgentA.AddReward(0.3f);

            // Agent A double hit
            if (lastAgentHit == AgentRole.A)
            {
                AgentBWins();
            }
            else
            {
                // agent can return serve in the air
                if (lastFloorHit != FloorHit.Service && !net)
                {
                    net = true;
                }

                lastAgentHit = AgentRole.A;
                lastFloorHit = FloorHit.FloorHitUnset;
            }
        }
        else if (collision.gameObject.name == "AgentB")
        {
            m_AgentB.AddReward(0.3f);

            // Agent B double hit
            if (lastAgentHit == AgentRole.B)
            {
                AgentAWins();
            }
            else
            {
                if (lastFloorHit != FloorHit.Service && !net)
                {
                    net = true;
                }

                lastAgentHit = AgentRole.B;
                lastFloorHit = FloorHit.FloorHitUnset;
            }
        }
    }
}
