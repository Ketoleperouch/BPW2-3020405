using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class WaypointSystem : MonoBehaviour {

    public Transform waypoint;
    public GameObject marker;
    public Sprite onScreen;
    public Sprite offScreen;
    public Text distanceIndicator;

    public Transform currentWaypoint { get; set; }
    public bool active { get; set; }
    public bool disableZoom { get; private set; }

    private Vector2 originalSize;
    private PlayerController player;

    public static WaypointSystem system;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        originalSize = marker.GetComponent<RectTransform>().sizeDelta;
        system = this;
        if (!currentWaypoint)
        {
            currentWaypoint = waypoint;
        }
    }

    private void LateUpdate()
    {
        ShowMarkers();
    }

    //For use through UnityEvents
    public void SetWaypoint(Transform newWaypoint)
    {
        active = true;
        currentWaypoint.position = newWaypoint.position;
        system.waypoint = currentWaypoint;
        StartCoroutine(InitializeMarker(marker.GetComponent<RectTransform>()));
    }
    public void SetWaypointPosition(Vector3 position)
    {
        active = true;
        currentWaypoint.position = position;
        system.waypoint = currentWaypoint;
        StartCoroutine(InitializeMarker(marker.GetComponent<RectTransform>()));
    }
    public void UnsetWaypoint()
    {
        active = false;
        system.waypoint = null;
    }

    private void ShowMarkers()
    {
        if (currentWaypoint == null || !active)
        {
            marker.SetActive(false);
            return;
        }
        else
        {
            waypoint = currentWaypoint;
            marker.SetActive(true);
        }
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(waypoint.transform.position);

        //Set marker size
        RectTransform markerTransform = marker.GetComponent<RectTransform>();
        if (!disableZoom)
        {
            markerTransform.sizeDelta = originalSize * MarkerSize;
        }
        //Check if on screen or offscreen
        if (screenPosition.z > 0 && screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height)
        {
            //On screen, display on screen marker
            marker.GetComponent<Image>().sprite = onScreen;
            markerTransform.rotation = Quaternion.identity;
            markerTransform.position = screenPosition;
            distanceIndicator.text = (Mathf.Round(Vector3.Distance(player.transform.position, currentWaypoint.position))).ToString() + " m";
        }
        else
        {
            //Offscreen

            Vector3 screenCenter = new Vector3(Screen.width, Screen.height) / 2;

            //Set {0, 0} as the screen center
            screenPosition -= screenCenter;

            //Reverse marker if target is behind player
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            //Magic stuff and dangerous mathematics
            float relativeAngle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            relativeAngle -= 90 * Mathf.Deg2Rad;

            float cosAngle = Mathf.Cos(relativeAngle);
            float sinAngle = -Mathf.Sin(relativeAngle);

            screenPosition = screenCenter + new Vector3(sinAngle * 150, cosAngle * 150);

            float m = cosAngle / sinAngle;

            Vector3 screenBounds = screenCenter * 0.9f;

            //Check if target is up or down out of screen bounds
            screenPosition = new Vector3(cosAngle > 0 ? screenBounds.y / m : -screenBounds.y / m, cosAngle > 0 ? screenBounds.y : -screenBounds.y);

            //Check if target is left or right out of screen bounds
            if (screenPosition.x > screenBounds.x)
            {
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * m);
            }
            else if (screenPosition.x < -screenBounds.x)
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * m);
            }

            //Set {0, 0} back to normal
            screenPosition += screenCenter;

            //Display out-of-bounds marker
            marker.GetComponent<Image>().sprite = offScreen;
            markerTransform.position = screenPosition;
            markerTransform.rotation = Quaternion.Euler(0, 0, relativeAngle * Mathf.Rad2Deg);

            distanceIndicator.text = "";
        }
    }

    private float MarkerSize
    {
        get
        {
            return Mathf.Max(0.95f, 2 - (Vector3.Distance(player.transform.position, waypoint.position) / 20));
        }
    }

    private IEnumerator InitializeMarker(RectTransform markerTransform)
    {
        float sizer = 0.02f;
        disableZoom = true;
        while (!Mathf.Approximately(markerTransform.sizeDelta.x, originalSize.x * MarkerSize))
        {
            markerTransform.sizeDelta = Vector2.Lerp(Vector2.one * 100, originalSize * MarkerSize, sizer);
            sizer *= 1.2f;
            yield return null;
        }
        disableZoom = false;
    }

}
