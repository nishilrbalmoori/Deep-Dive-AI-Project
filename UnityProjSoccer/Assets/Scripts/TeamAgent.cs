using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;


public class TeamAgent : Agent
{
    [Header("Requirements")]
    public TeamController team;
    public BallController ball;

    public NetController teamGoal, opGoal;

    [Header("Reward Settings")]
    [SerializeField] private float 
    goalReward = 10f, 
    possessionReward = 0.01f, 
    shotReward = 0.5f, 
    timePenalty = -0.001f,
    switchReward = 0.25f,

    ballDistRewardScale = 5f,
    goalDistRewardScale = 50f;

    public const float optimalShootingDistance = 10f;

    [Header("Stats")]
    [SerializeField] private int goalsScored = 0, goalsConceded = 0, shotsOnGoal = 0;

    public override void Initialize()
    {
        InitML();
    }

    private void InitML()
    {
        foreach(GameObject player in team.players)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.useML = true;
        }
    }
    
    private void Reset()
    {
        team.Reset();
        ball.Reset();

        goalsScored = 0;
        goalsConceded = 0;
        shotsOnGoal = 0; 
    }
    
    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AddBallObserv(sensor);
        AddPlayersObserv(sensor);
        AddOpGoalObserv(sensor);
        AddTeamGoalObserv(sensor);

        sensor.AddObservation((ball.GetTeamWithPossesion() == team) ? 1.0f : 0.0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
        int continuousActionIndex = 0;
        int discreteActionIndex = 0;
    
        foreach (GameObject playerObj in team.players)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            Rigidbody rb = pc.rb;

            if(pc.isActive){
                discreteActionIndex = MoveActionAndReward(actions, continuousActionIndex, discreteActionIndex, pc, rb);
                discreteActionIndex = ShootActionAndReward(actions, discreteActionIndex, pc);
                discreteActionIndex = SwitchActivePlayerAndReward(actions, discreteActionIndex, pc);
            }
        }

        AddIdleRewards();

    }

    private int MoveActionAndReward(ActionBuffers actions, int continuousActionIndex, int discreteActionIndex,PlayerController pc, Rigidbody rb)
    {
        Vector3 moveDir = new Vector3(
            Mathf.Clamp(actions.ContinuousActions[continuousActionIndex++], -1f, 1f), 
            0, 
            Mathf.Clamp(actions.ContinuousActions[continuousActionIndex++], -1f, 1f)
        );

        int sprintAction = actions.DiscreteActions[discreteActionIndex++];

        if (moveDir.magnitude > 0.1f){
            moveDir = moveDir.normalized;

            float speed = sprintAction > 0 ? PlayerController.sprintSpeed : PlayerController.runSpeed;
            rb.linearVelocity = moveDir * speed;
            
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            pc.transform.rotation = Quaternion.Slerp(pc.transform.rotation, targetRot, Time.deltaTime * 10f);
        }
        else rb.linearVelocity = Vector3.zero; 

        Vector3 dispPlayer2Ball = pc.transform.position - ball.transform.position;
        Vector3 dispPlayer2OpGoal = pc.transform.position - opGoal.transform.position;
        Vector3 dispPlayer2TeamGoal = pc.transform.position - teamGoal.transform.position;

        float similarity2Ball = Vector3.Dot(dispPlayer2Ball.normalized, moveDir.normalized);
        float similarity2OpGoal = Vector3.Dot(dispPlayer2OpGoal.normalized, moveDir.normalized);
        float similarity2TeamGoal = Vector3.Dot(dispPlayer2TeamGoal.normalized, moveDir.normalized);

        AddReward((1 - similarity2Ball)/ballDistRewardScale); 
        AddReward((1 - ((pc.role == TeamController.ATTACKER) ? similarity2OpGoal : similarity2TeamGoal))/goalDistRewardScale);

        return discreteActionIndex;
    }

    private int ShootActionAndReward(ActionBuffers actions, int discreteActionIndex, PlayerController pc)
    {
        int kickAction = actions.DiscreteActions[discreteActionIndex++];

        if (kickAction > 0 && ball.HasPossession(pc)){
            Vector3 kickDir = (opGoal.transform.position - pc.transform.position).normalized;
            ball.ExecuteKick(kickDir, 1.2f); 

            Vector3 distBall2OpGoal = ball.transform.position - opGoal.transform.position;

            if(distBall2OpGoal.magnitude <= optimalShootingDistance){
                AddReward(shotReward);
                shotsOnGoal++;
            }
        }

        return discreteActionIndex;
    }
    public override void Heuristic(in ActionBuffers actionsOut){
        var continuous = actionsOut.ContinuousActions;
        var discrete = actionsOut.DiscreteActions;
        int cont_idx = 0;
        int desc_idx = 0;
        

        continuous[cont_idx++] = Input.GetAxis("Horizontal");
        continuous[cont_idx++] = Input.GetAxis("Vertical");
        discrete[desc_idx++] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0; 
        discrete[desc_idx++] = Input.GetKey(KeyCode.Space) ? 1 : 0;    
        discrete[desc_idx++] = Input.GetKey(KeyCode.Return) ? 1 : 0;           
    }

    private int SwitchActivePlayerAndReward(ActionBuffers actions, int discreteActionIndex, PlayerController pc)
    {
        int switchPlayer = actions.DiscreteActions[discreteActionIndex++];

        if(switchPlayer > 0){
            float oldDistance =
                Vector3.Distance(
                    pc.transform.position,
                    ball.transform.position
                );

            team.SwitchActivePlayer();

            float newDistance =
                Vector3.Distance(
                    team.activePlayer.transform.position,
                    ball.transform.position
                );

            if(newDistance < oldDistance)
                AddReward(switchReward);
            else
                AddReward(-switchReward);
        }
        return discreteActionIndex;
    }

    private void AddIdleRewards()
    {
        if(team.mlScoreDetected) {
            AddReward(goalReward);
            goalsScored++;
            team.mlScoreDetected = false;
            EndEpisode();
        }

        else if(team.mlConcededDetected) {
            AddReward(-goalReward);
            goalsConceded++;
            team.mlConcededDetected = false;
            EndEpisode();
        }
        if (ball.GetTeamWithPossesion() == team) AddReward(possessionReward * Time.deltaTime); 
        AddReward(timePenalty);
    }
    private void AddBallObserv(VectorSensor sensor)
    {
        Vector3 scaledPos = ScalePos(ball.transform.position);
        Vector3 scaledVel = ScaleVel(ball.rb.linearVelocity);

        sensor.AddObservation(scaledPos.x);
        sensor.AddObservation(scaledPos.z);

        sensor.AddObservation(scaledVel.x);
        sensor.AddObservation(scaledVel.z);
    }

    private void AddPlayersObserv(VectorSensor sensor)
    {

        Vector3 center = Vector3.zero;
        foreach(GameObject player in team.players)
        {
            Vector3 scaledPos = ScalePos(player.transform.position);
            Vector3 scaledVel = ScaleVel(player.GetComponent<PlayerController>().rb.linearVelocity);
            
            sensor.AddObservation(scaledPos.x);
            sensor.AddObservation(scaledPos.z);

            sensor.AddObservation(scaledVel.x);
            sensor.AddObservation(scaledVel.z);

            center += player.transform.position;
        }

        center /= (TeamController.ATTACKER_COUNT + TeamController.DEFENDER_COUNT + 1);
        center = ScalePos(center);

        sensor.AddObservation(center.x);
        sensor.AddObservation(center.z);

        foreach(GameObject player in team.players)
        {
            sensor.AddObservation(
                player == team.activePlayer ? 1f : 0f
            );
        }
    }

    private void AddOpGoalObserv(VectorSensor sensor)
    {
        Vector3 scaledDist = ScalePos(opGoal.transform.position - ball.transform.position);

        sensor.AddObservation(scaledDist.x);
        sensor.AddObservation(scaledDist.z);
    }

    private void AddTeamGoalObserv(VectorSensor sensor)
    {
        Vector3 scaledDist = ScalePos(teamGoal.transform.position - ball.transform.position);

        sensor.AddObservation(scaledDist.x);
        sensor.AddObservation(scaledDist.z);
    }

    private Vector3 ScalePos(Vector3 vec)
    {
        Vector3 toReturn = new Vector3(2*(vec.x - 48f)/(164f - 48f) - 1, vec.y, 2*(vec.z + 35f)/(-100f + 35f) - 1);
        return toReturn;
    }

    private Vector3 ScaleVel(Vector3 vec)
    {
        return vec/20f;
    }

    void Update()
    {
        RequestDecision();
    }
}
