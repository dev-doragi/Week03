using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    [SerializeField] private CameraBoundsController boundsController;

    public void ApplyZone(CameraZone zone)
    {
        if (zone == null)
        {
            boundsController.ClearBounds();
            return;
        }

        boundsController.SetBounds(zone.MinBounds, zone.MaxBounds, zone.UseBounds, zone.CorrectionX, zone.CorrectionY);
    }
}
