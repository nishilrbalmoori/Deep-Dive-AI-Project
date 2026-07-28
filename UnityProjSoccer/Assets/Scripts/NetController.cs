using UnityEngine;

public class NetController : MonoBehaviour
{
    public TeamController team1Controller, team2Controller;
    public BallController ballController;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            if(gameObject.name == "Opposing Net") {
                team1Controller.OnScore();
                team2Controller.OffScore();
            }
            else
            {
                team2Controller.OnScore();
                team1Controller.OffScore();
            }

            Invoke("ResetGame", 0.5f);
        }
    }

    void ResetGame()
    {
        team1Controller.Reset();
        team2Controller.Reset();
        ballController.Reset();
    }
}
