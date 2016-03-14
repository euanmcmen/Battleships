using UnityEngine;

//This class handles the adaptability of the scene by activating a set of game objects - known as a collection - depending on the active platform.
public class AdaptabilityScript : MonoBehaviour 
{
    //The collection objects which will be assigned in the inspector.
    public GameObject desktopCollectionGO;

    //The mobile device collection objects.
    public GameObject portraitMobileCollectionGO;
    public GameObject landscapeMobileCollectionGO;

    //The collection object to be used for the current platform.  
    protected GameObject activeCollection;

    // Use this for initialization
    public void Start () 
    {
        //Depending on the platform, initialise the platform.
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        {
            //Assign the active canvas to the desktop canvas.
            activeCollection = desktopCollectionGO;
        }
        #endif

        #if UNITY_ANDROID
        {
            //Check the orentation of the device.
            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                activeCollection = portraitMobileCollectionGO;
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                activeCollection = landscapeMobileCollectionGO;
            }

            //Lock rotation of the device after the rotation has complete.
            SetAutoRotationCapability(false);
        }
        #endif

        //After initialising the collections and assigning one based on platform, set it to active.
        activeCollection.SetActive(true);
    }

    //This method locks or unlocks autorotation of an android device.
    void SetAutoRotationCapability(bool allowRotation)
    {
        Screen.autorotateToLandscapeLeft = allowRotation;
        Screen.autorotateToLandscapeRight = allowRotation;
        Screen.autorotateToPortrait = allowRotation;
    }
}
