using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon;

//This script controls the flow of the setup phase by placing ships and listening for the ready signal from both players.
public class SetupControlScript : Photon.MonoBehaviour
{
    //The array of ships
    GameObject[] shipObjects;

    //The selected ship in the setup phase of the game.
    //The property uses get and set methods to control which ship is selected.
    //When changing the value of the selected ship, the previously selected ship will have its value unset.
    ShipSetupScript privateSelectedShip = null;
    ShipSetupScript selectedShip
    {
        get
        {
            //Return the selected ship.
            return privateSelectedShip;
        }
        set
        {
            //Deselct the ship if one is currently selected.
            if (privateSelectedShip != null)
            {
                privateSelectedShip.DeselectShip();
                privateSelectedShip = null;
            }

            //Select the new ship.
            privateSelectedShip = value;

            //If the new value is not null, select that ship.
            if (privateSelectedShip != null)
                privateSelectedShip.SelectShip();
        }
    }

    //A read-only property which returns whether a ship is selected.
    bool isShipSelected
    {
        get
        {
            return selectedShip != null;
        }
    }

    //The timer controller
    TimerScript timerController;

    //This is the control script for the overlays.
    OverlayScript overlayController;

    //UI ship movement for Android platforms only.
#if UNITY_ANDROID
    Button upButton;
    Button downButton;
    Button leftButton;
    Button rightButton;
    Button rotateButton;
    bool buttonsEnabled = false;
#endif

    //The local and other players from the perspective of this instance.
    //These will reference the photonplayer objects wrapped by the global references in the playermanager.
    Player localPlayer = null;
    Player otherPlayer = null;

    //A flag to indicate that the rpc has been made and should not be exectuted again.
    bool fleetPositionRPCMade = false;

    //This flag determines whether the game should start anyway without user ready state. 
    //This is set by the forcestart method called from the timer.
    bool shouldForceStart = false;

    // Use this for initialization
    void Start()
    {
        //Declare the serialization method for the fleet.
        PhotonPeer.RegisterType(typeof(Fleet), (byte)'F', Fleet.Serialize, Fleet.Deserialize);

        //Get attached components.
        overlayController = GetComponent<OverlayScript>();
        timerController = GetComponent<TimerScript>();

        //Get the local player from the player manager.
        //This player describes the current instance and will share an id with one of the playermanager instances.
        localPlayer = PlayerManager.GetLocalPlayer();

        //Get the other player by checking the id of the local player.
        //This player describes the other instance and will share an id with one of the playermanager instances.
        otherPlayer = PlayerManager.GetOtherPlayer();

        //Get the structure of the spawn.
        localPlayer.fleet = ShipManager.GetSpawnPositions();

        //Spawn each ship.
        shipObjects = new GameObject[3];
        shipObjects[0] = Instantiate(Resources.Load<GameObject>("Setup/Ships/SmallShip"), localPlayer.fleet.smallShip.position, Quaternion.Euler(0, 0, (int)localPlayer.fleet.smallShip.rotation)) as GameObject;
        shipObjects[1] = Instantiate(Resources.Load<GameObject>("Setup/Ships/MediumShip"), localPlayer.fleet.mediumShip.position, Quaternion.Euler(0, 0, (int)localPlayer.fleet.mediumShip.rotation)) as GameObject;
        shipObjects[2] = Instantiate(Resources.Load<GameObject>("Setup/Ships/LargeShip"), localPlayer.fleet.largeShip.position, Quaternion.Euler(0, 0, (int)localPlayer.fleet.largeShip.rotation)) as GameObject;

        //Return these spawned ships back to the ship manager.
        ShipManager.WriteShipsToManager(shipObjects);

        //Set up the ready button.
        //This will bring up an overlay to tell the player the game is waiting.
        Button readyBtn = GameObject.Find("ReadyButton").GetComponent<Button>();
        readyBtn.onClick.AddListener(() =>
        {
            //Set the local player's ready state to true to false on both platforms.
            SetReadyStatus(true);

            //Show waiting overlay.
            overlayController.ShowOverlay(0);

            //Set the active ship to null.
            selectedShip = null;

            //Set the cancel button.
            Button cancelButton = GameObject.Find("CancelButton").GetComponent<Button>();

            cancelButton.onClick.AddListener(() =>
            {
                //Set the local player's ready state to false on both platforms.
                SetReadyStatus(false);

                //Hide the overlays and return to game.
                overlayController.HideAll();
            });
        });

        //If the current platform is a mobile platform, then setup the ship interaction buttons.
        //These do not appear on the desktop platform and so are skipped.
#if UNITY_ANDROID
        {
            //Set up the Up button to move the selected ship up.
            upButton = GameObject.Find("UpButton").GetComponent<Button>();
            upButton.onClick.AddListener(() =>
            {
                selectedShip.MoveShip(Vector2.up);
            });

            //Set up the Down button to move the selected ship down.
            downButton = GameObject.Find("DownButton").GetComponent<Button>();
            downButton.onClick.AddListener(() =>
            {
                selectedShip.MoveShip(Vector2.down);
            });

            //Set up the Left button to move the selected ship left.
            leftButton = GameObject.Find("LeftButton").GetComponent<Button>();
            leftButton.onClick.AddListener(() =>
            {
                selectedShip.MoveShip(Vector2.left);
            });

            //Set up the Right button to move the selected ship right.
            rightButton = GameObject.Find("RightButton").GetComponent<Button>();
            rightButton.onClick.AddListener(() =>
            {
                selectedShip.MoveShip(Vector2.right);
            });

            //Set up the Rotate button to rotate selected the ship.
            rotateButton = GameObject.Find("RotateButton").GetComponent<Button>();
            rotateButton.onClick.AddListener(() =>
            {
                selectedShip.RotateShip();
            });
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        //When both players are ready, we lock in the ships and get ready to play.
        //This method will also be invoked when the timer expires.
        if ((localPlayer.isReady && otherPlayer.isReady) || shouldForceStart)
        {
            //Stop the timer.
            if (timerController.isRunning)
                timerController.StopTimer(false);

            //Show the starting overlay.
            overlayController.ShowOverlay(1);

            //Set the fleet positions.  
            //Don't allow this method to be called more than once.
            if (!fleetPositionRPCMade)
            {
                localPlayer.fleet = GetFleet();
                photonView.RPC("SendShips", otherPlayer.GetPhotonPlayer());
                fleetPositionRPCMade = true;
            }

            //We can transition to the next scene and actually begin the game if neither of the players fleets are null
            //This "waiting" is for the network to recieve the opponent's fleet positions.
            if (localPlayer.fleet != null && otherPlayer.fleet != null && !IsInvoking("OpenGameScene"))
            {
                //Load the game from the master client after one second.
                //Scene syncing will carry any other clients along.
                if (PhotonNetwork.isMasterClient)
                {
                    Invoke("OpenGameScene", 1);
                }
            }
        }

#if UNITY_ANDROID
        //Select a ship when it is touched, otherwise select nothing.
        //Also make sure the user is selecting a game object which does not use the event system.
        if (!localPlayer.isReady)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(0))
                {
                    //Initialise the raycast.
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

                    //Cast the ray and check if it hits a ship.
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        if (hit.collider.tag == "Ship")
                            selectedShip = hit.collider.gameObject.GetComponent<ShipSetupScript>();

                        //If the user hits the background object, then unselect the ship.
                        else if (hit.collider.tag == "Background")
                            selectedShip = null;
                    }
                }
            }
        }

        //If no ship is selected, then disable the buttons.  Enable it if one is selected.
        if (isShipSelected && !buttonsEnabled)
        {
            upButton.interactable = true;
            downButton.interactable = true;
            leftButton.interactable = true;
            rightButton.interactable = true;
            rotateButton.interactable = true;
            buttonsEnabled = true;
        }

        if (!isShipSelected && buttonsEnabled)
        {
            upButton.interactable = false;
            downButton.interactable = false;
            leftButton.interactable = false;
            rightButton.interactable = false;
            rotateButton.interactable = false;
            buttonsEnabled = false;
        }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        //If the local player can recieve input, allow the player to select a ship.
        if (!localPlayer.isReady)
        {
            //Make sure the user is selecting a game object which does not use the event system.
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(-1))
            {
                //Initialise the raycast.
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                //Cast the ray and check if it hits a ship.
                if (Physics.Raycast(ray, out hit, 100))
                    selectedShip = (hit.collider.tag == "Ship") ? hit.collider.gameObject.GetComponent<ShipSetupScript>() : null;
            }
        }

        //If the local player can recieve input and there is a ship selected, allow the ships to move.
        if (!localPlayer.isReady && isShipSelected)
        {
            //Move with the arrow keys and the r key.
            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedShip.MoveShip(Vector2.down);

            else if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedShip.MoveShip(Vector2.up);

            else if(Input.GetKeyDown(KeyCode.LeftArrow))
                selectedShip.MoveShip(Vector2.left);

            else if(Input.GetKeyDown(KeyCode.RightArrow))
                selectedShip.MoveShip(Vector2.right);

            else if(Input.GetKeyDown(KeyCode.R))
                selectedShip.RotateShip();
        }

        //If the local player can recieve input, allow a ship to be selected using numbers 1, 2, or 3.
        if (!localPlayer.isReady)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                selectedShip = shipObjects[0].GetComponent<ShipSetupScript>();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                selectedShip = shipObjects[1].GetComponent<ShipSetupScript>();
            if (Input.GetKeyDown(KeyCode.Alpha3))
                selectedShip = shipObjects[2].GetComponent<ShipSetupScript>();
        }



#endif

        //Quit when the back button or escape is pressed.
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }

    //This method sets a variable which will force the game to sync and start anyway.
    //This is called from the timer control script.
    public void ForceStart()
    {
        shouldForceStart = true;
    }

    //Launches the game scene.  This is called in an invoke method to start after a second.
    //This allows extra time for the rpc calls to finish and prevents desync errors.
    void OpenGameScene()
    {
        PhotonNetwork.LoadLevel(2);
    }

    //This method syncronizes the ready states of the players across the platform.
    //Local player == Otherplayer's Otherplayer.
    void SetReadyStatus(bool isReady)
    {
        //Set the active player's ready state.
        localPlayer.isReady = isReady;

        //Set the ready state of the other player's otherplayer.
        photonView.RPC("SetOtherPlayerReadyState", otherPlayer.GetPhotonPlayer(), isReady);
    }

    //Returns a fleet object of the local player's ships.
    Fleet GetFleet()
    {
        //Create a ship positions array of the fleet positions.
        ShipLocation[] shipPositions = new ShipLocation[shipObjects.Length];
        for (int i = 0; i < shipObjects.Length; i++)
            shipPositions[i] = new ShipLocation(shipObjects[i].transform.position, (ShipOrientation)shipObjects[i].transform.eulerAngles.z);

        //Return a new fleet created from the positions.
        return new Fleet(shipPositions[0], shipPositions[1], shipPositions[2]);
    }

    //This method is executed on the other player's client.
    //It calls another rpc to set the locations of their (the other player's) fleet 
    [PunRPC]
    void SendShips()
    {
        //Get the fleet positions
        Fleet fleet = GetFleet();

        //Call the method on the other player's client. (technically the local client from the local player's perspective.)
        photonView.RPC("SetOtherPlayerFleetPositions", otherPlayer.GetPhotonPlayer(), fleet);
    }

    //This methods is called from the other client onto this client.
    [PunRPC]
    void SetOtherPlayerFleetPositions(Fleet otherPlayerPositions)
    {
        //Tell the other client to send their ships to the local player.
        otherPlayer.fleet = otherPlayerPositions;
    }

    //Sets the status of the other player from the perspective of the other player.
    //This syncronises the local player ready states.
    [PunRPC]
    void SetOtherPlayerReadyState(bool isReady)
    {
        //This will be called on the other platform to set the local variable of otherplayer.
        otherPlayer.isReady = isReady;
    }

    //This is called when a user presses the return to menu ui button.
    //It presents the user with an overlay which will allow them to disconnect and return to menu.
    public void MenuUIButton()
    {
        //Show return to menu panel
        overlayController.ShowOverlay(3);
    }

    //Returns the user to the menu and disconnects them from photon.
    //This is called when a return to menu overlay button is pressed or the user is disconnected and presses the overlay button.
    public void ReturnToMenuButton()
    {
        //Disconnect if the player hasn't already disconnected.
        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        //Return to menu.
        PhotonNetwork.LoadLevel(0);
    }

    //This hides any overlays and returns the player to the game.
    //This is called when the cancel button of the menu ui overlay is pressed.
    public void CancelButton()
    {
        //Hide all overlays.
        overlayController.HideAll();
    }

    //If a player leaves the game, display the disconnection overlay.
    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        //Stop the timer.
        timerController.StopTimer(false);

        //Disconnect from photon.
        PhotonNetwork.Disconnect();

        //Show a message that the player has abandoned the game.
        overlayController.ShowOverlay(2);
    }
}
