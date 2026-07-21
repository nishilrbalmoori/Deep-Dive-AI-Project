using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class TeamController : MonoBehaviour
{
    public CinemachineCamera camera;
    private List<GameObject> players = new List<GameObject>();
    void Start()
    {        

        //Init Players In A Team
        foreach(Transform child in transform){
            if (child.TryGetComponent<PlayerController>(out PlayerController script)){
                players.Add(child.gameObject);
            }
        }

        FollowActivePlayer();
    }

    void Update()
    {
        SwitchActivePlayer();
    }

    private void SwitchActivePlayer()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            GameObject active = null;
            Dictionary<GameObject, float> dists = new Dictionary<GameObject, float>();


            foreach(GameObject player in players){
                if (player.GetComponent<PlayerController>().isActive) active = player;
            }            

            foreach(GameObject player in players)
            {
                if(player.GetEntityId().GetHashCode() != active.GetEntityId().GetHashCode())
                {
                    dists.Add(player, Vector3.Distance(active.transform.position, player.transform.position));
                }
            }

            GameObject min = null;

            foreach(var(id, dist) in dists)
            {
                if(min == null || dists[id] < dists[min]) min = id;
            }

            PlayerController active_2_un = active.GetComponent<PlayerController>();
            PlayerController un_2_active = min.GetComponent<PlayerController>();

            active_2_un.isActive = false;
            un_2_active.isActive = true;

            FollowActivePlayer();
        }
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
}
