using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class TeamController : MonoBehaviour
{
    public const string ATTACKER = "Attacker";
    public const string DEFENDER = "Defender";
    public const string GOALIE = "Goalie";

    public const int ATTACKER_COUNT = 2;
    public const int DEFENDER_COUNT = 2;

    public GameObject playerPositionings;

    public CinemachineCamera camera;
    public Transform ball;
    public List<GameObject> players = new List<GameObject>();
    public PlayerController activePlayer;

    public bool isActive = false;

    public bool useML = false, mlScoreDetected = false, mlConcededDetected = false;
    void Start()
    {        
        InitPlayers();

        if(isActive) FollowActivePlayer();
    }

    private void InitPlayers()
    {
        int aCount = 0, dCount = 0;
        foreach(Transform child in transform){
            if (child.TryGetComponent<PlayerController>(out PlayerController script)){
                script.role = (aCount < ATTACKER_COUNT) ? ATTACKER : (dCount < DEFENDER_COUNT) ? DEFENDER : GOALIE;
                script.rolePosition = (aCount < ATTACKER_COUNT) ? aCount++ : (dCount < DEFENDER_COUNT) ? dCount++ : 0;
                script.playerPositionings = playerPositionings;
                script.team = this;
                script.useML = useML;

                players.Add(child.gameObject);
                script.Reset();
            }
        }
    }

    void Update()
    {
        if(isActive) SwitchActivePlayer();

    }

    public void SwitchActivePlayer()
    {
        if (Input.GetKeyDown(KeyCode.Return)){
            GameObject active = null;
            


            foreach(GameObject player in players){
                if (player.GetComponent<PlayerController>().isActive) active = player;
            }            

            GameObject min = GetClosetPlayer();


            PlayerController active_2_un = active.GetComponent<PlayerController>();
            PlayerController un_2_active = min.GetComponent<PlayerController>();

            if(un_2_active.id != active_2_un.id)
            {
                active_2_un.isActive = false;
                un_2_active.isActive = true;

                FollowActivePlayer(min);
            }
        }
    }

    public GameObject GetClosetPlayer()
    {
        Dictionary<GameObject, float> dists = new Dictionary<GameObject, float>();
        foreach(GameObject player in players)
        {
            if(player.GetComponent<PlayerController>().id != activePlayer.id) dists.Add(player, Vector3.Distance(player.transform.position, ball.position));
        }

        GameObject min = null;

        foreach(var(id, dist) in dists)
        {
            if(min == null || dists[id] < dists[min]) min = id;
        }

        return min;
    }

    private void FollowActivePlayer(GameObject player)
    {
        camera.Target.TrackingTarget = player.transform;
        activePlayer = player.GetComponent<PlayerController>();
    }
    private void FollowActivePlayer()
    {
        foreach(GameObject player in players)
        {
            if (player.TryGetComponent<PlayerController>(out PlayerController script)){
                if (script.isActive)
                {
                    camera.Target.TrackingTarget = player.transform;
                    return;
                }
            }
        }
    }

    public void Reset()
    {
        foreach(GameObject player in players){
            if (player.TryGetComponent<PlayerController>(out PlayerController script)) script.Reset();
        }
    }

    public void OnScore()
    {
        if (useML) mlScoreDetected = true;
    }

    public void OffScore()
    {
        if (useML) mlConcededDetected = true;
    }
}
