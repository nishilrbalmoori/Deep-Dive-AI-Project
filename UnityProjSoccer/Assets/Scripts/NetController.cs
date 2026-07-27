using UnityEngine;

public class Netcontroller : MonoBehaviour
{
    public TeamController team1Controller;
    public BallController ballController;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            team1Controller.Reset();
            ballController.Reset();
        }
    }
}
