using UnityEngine;

public class OverlayScript : MonoBehaviour
{
    //A collection of overlays to be displayed.
    GameObject[] overlays;

    //The input blocker and overlay parent object.
    GameObject overlayParentObject;

    // Use this for initialization
    void Start()
    {
        //Fill the overlays array from the overlay container.
        //Even though the "parent" of the overlays will be the input blocker, the overlay container should be the active object which houses the overlays.
        //Therefore, the overlays will be the children of the child object from the overlayContainer.

        //Get the overlay container transform and attach it to the canvas.
        Transform overlayContainer = GameObject.Find("OverlayContainer").transform;
        overlayContainer.SetParent(GameObject.Find("Canvas").transform);

        //Correct the overlay container's position and scale after setting new parent.
        overlayContainer.localScale = new Vector3(1, 1, 1);
        overlayContainer.localPosition = Vector3.zero;

        //Get the parent object of the overlays.
        overlayParentObject = overlayContainer.GetChild(0).gameObject;

        //Initialise the array.
        overlays = new GameObject[overlayParentObject.transform.childCount];

        //Get the children of the parent object and place them in the array.
        for (int i = 0; i < overlays.Length; i++)
        {
            overlays[i] = overlayParentObject.transform.GetChild(i).gameObject;
        }
	}

    void HideAllOverlays()
    {
        //Hide all overlay objects in the game object.
        foreach (GameObject go in overlays)
        {
            go.SetActive(false);
        }
    }

    public void HideAll()
    {
        //Hide the overlays.
        HideAllOverlays();

        //Hide the parent.
        overlayParentObject.SetActive(false);
    }

    public void ShowOverlay(int index)
    {
        //Hide the overlays.
        HideAllOverlays();

        //Show the parent input blocker.
        overlayParentObject.SetActive(true);

        //Then reveal the desired overlay.
        overlays[index].SetActive(true);
    }
}
