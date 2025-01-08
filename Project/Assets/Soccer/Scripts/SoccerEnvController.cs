using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoccerEnvController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI blueScoreText;
    [SerializeField] private TextMeshProUGUI purpleScoreText;
    [SerializeField] private Renderer FieldArea;
    [SerializeField] private GameObject golPanel;
    [SerializeField] private TextMeshProUGUI startingText;
    [SerializeField] private Material[] materials;
    private Material fieldAreaMaterial;
    [SerializeField] private GameObject[] agents;
    private int timer;

    private bool isPaused = false;
    int blueScore = 0;
    int purpleScore = 0;
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }


    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;

    void Start()
    {
        StartCoroutine(StartingText());
        fieldAreaMaterial = FieldArea.material;
        StartCoroutine(Timer());
        PauseTrainingAndEnvironment();
        Invoke("ResumeTrainingAndEnvironment", 4f);
        timerText.text = timer.ToString();
        blueScoreText.text = blueScore.ToString();
        purpleScoreText.text = purpleScore.ToString();


        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        ResetScene();
    }

    IEnumerator StartingText()
    {
        startingText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            startingText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        startingText.text = "GO!";
        yield return new WaitForSeconds(1);
        startingText.gameObject.SetActive(false);
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(4);
        for (int i = 0; i < 90; i++)
        {
            yield return new WaitForSeconds(1);
            timer++;
            timerText.text = timer.ToString();
        }
        SetWinner();
        SceneManager.LoadScene(2);
        PauseTrainingAndEnvironment();
    }

    public void SetWinner()
    {
        if (blueScore > purpleScore)
        {
            PlayerPrefs.SetInt("Winner", 0);
        }
        else if (purpleScore > blueScore)
        {
            PlayerPrefs.SetInt("Winner", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Winner", 2);
        }
    }

    public void PauseTrainingAndEnvironment()
    {

        isPaused = true;
        foreach (var item in agents)
        {
            item.GetComponent<AgentSoccer>().PauseAgent();
        }
    }

    public void ResumeTrainingAndEnvironment()
    {
        isPaused = false;
        foreach (var item in agents)
        {
            item.GetComponent<AgentSoccer>().ResumeAgent();
        }
    }

    private void ChangeMaterial()
    {
        FieldArea.material = fieldAreaMaterial;
    }


    void FixedUpdate()
    {
        if (isPaused)
        {
            return;
        }
        m_ResetTimer += 1;

        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }


    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void GoalTouched(Team scoredTeam)
    {

        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
            blueScore++;
            blueScoreText.text = blueScore.ToString();
            FieldArea.material = materials[0];
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
            purpleScore++;
            purpleScoreText.text = purpleScore.ToString();
            FieldArea.material = materials[1];
        }
        golPanel.GetComponent<Image>().DOFade(1, 0.5f);
        golPanel.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1f, 10, 1).OnComplete(() => golPanel.GetComponent<Image>().DOFade(0, .5f));

        Invoke("ChangeMaterial", 0.20f);

        PauseTrainingAndEnvironment();
        Invoke("ResumeTrainingAndEnvironment", 1.5f);
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();

    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.linearVelocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        ResetBall();
    }
}
