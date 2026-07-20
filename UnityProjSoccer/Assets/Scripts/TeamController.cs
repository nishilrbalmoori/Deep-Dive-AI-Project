using UnityEngine;
using System.Collections.Generic;

public class TeamController : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();
    void Start()
    {        
        foreach(Transform child in transform){
            if (child.TryGetComponent<PlayerController>(out PlayerController script)){
                players.add(child);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Enter)){
            GameObject active = null;
            Dictionary<GameObject, float> dists = new Dictionary<string, float>();


            foreach(GameObject player in players){
                if (player.GetComponent<PlayerController>().isActive()) active = player;
            }            

            foreach(GameObject player in players)
            {
                if(player.GetInstanceId() != active.GetInstanceId())
                {
                    dists.Add(player, Vector3.Distance(active.transform.position, player.transform.position));
                }
            }

            GameObject min_id = null;

            foreach(var(id, dist) in dists)
            {
                if(min_id == null || dists[id] < dists[min_id]) min_id = id;
            }

            active.GetComponent<PlayerController>.isActive = false;
            min_id.GetComponent<PlayerController>.isActive = true;
        }
    }
}
