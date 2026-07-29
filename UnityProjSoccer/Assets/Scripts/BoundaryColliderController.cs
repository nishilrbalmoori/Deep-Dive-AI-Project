using UnityEngine;

public class BoundaryColliderController : MonoBehaviour
{
    public TeamAgent team1, team2;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Ball"))
        {
            team1.EndEpisode();
            team2.EndEpisode();

            team1.AddReward(-0.5f);
            team2.AddReward(-0.5f);
        }
    }
}
