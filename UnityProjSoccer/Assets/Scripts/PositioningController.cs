using UnityEngine;

public class PositioningController : MonoBehaviour
{
   public PlayerController playerController;

    void OnTriggerExit(Collider other)
    {
        if(other == playerController.collider)
        {
            playerController.withinSoftBoundaries = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other == playerController.collider)
        {
            playerController.withinSoftBoundaries = true;
        }
    }
}
