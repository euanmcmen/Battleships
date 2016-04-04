using UnityEngine;
using UnityEngine.UI;

//This controls the menu.
//This script depends on the adaptability script enabling one of the collections, and so should be executed after the adaptability script.
public class MenuControlScript : Photon.MonoBehaviour 
{
    //Game information strings.
    const string GameVersion = "Hons_Battleships_1.0";

    //The text object attached to the root menu to give information regarding the status of Photon.
    Text connectionText = null;

    //Text object attached to the instructions panel.
    Text instructionsText = null;

    //The menu panel.
    GameObject menuPanel = null;

    //the waiting overlay.
    OverlayScript overlayController;

    // Use this for initialization
    void Start()
    {
        //Get the text and set its text component.
        connectionText = GameObject.Find("ConnectionText").GetComponent<Text>();
        connectionText.text = "CONNECTING TO PHOTON SERVER";

        //Get the other UI elements.
        menuPanel = GameObject.Find("MenuPanel").gameObject;
        instructionsText = GameObject.Find("OverlayContainer").transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();  //Overlay container > Inputblocker > Instruction panel > text component.

        //Set the instructions text depending on platform.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        instructionsText.text = "MENU PHASE\r\nClick the Play button to prepare for the Setup phase.  It will begin when the other player is ready.\r\n\r\nSETUP PHASE\r\nClick one of the three ships, or use numbers 1, 2, or 3, to select a ship.  Use the arrow keys to move it and press the R key to rotate it if there is room.\r\nClick the Ready button when ready and wait for the other player.\r\n\r\nATTACK PHASE\r\nClick on up to three tiles to select them, and click the Fire button to attack those tiles and end your turn.\r\nThe amount of tiles selectable depends on how many active ships you have in your fleet.\r\nThe game will continue until one player has no active ships in their fleet.\r\n\r\nDuring any phase, click the Menu button to return to menu, or press the Escape key to exit the application.";
#endif
#if UNITY_ANDROID
        instructionsText.text = "MENU PHASE\r\nTouch the Play button to prepare for the Setup phase.  It will begin when the other player is ready.\r\n\r\nSETUP PHASE\r\nTouch one of the three ships to select it.  Use the movement buttons to move it and rotate it within the grid if there is room.\r\nTouch the Ready button when ready and wait for the other player.\r\n\r\nATTACK PHASE\r\nTouch up to three tiles to select them, and touch the Fire button to attack those tiles and end your turn.\r\nThe amount of tiles selectable depends on how many active ships you have in your fleet.  The game will continue until one player has no active ships in their fleet.\r\n\r\nDuring any phase, touch the Menu button to return to menu, or press the back button on your device to exit the application.";
#endif

        //Get the overlay controller.
        overlayController = GetComponent<OverlayScript>();

        //Set the other elements to false.
        menuPanel.SetActive(false);

        //Set photon settings sync
        PhotonNetwork.automaticallySyncScene = true;

        //Connect to the photon server.
        PhotonNetwork.ConnectUsingSettings(GameVersion);
    }

    void Update()
    {
        //Quit when the back button or escape is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (PhotonNetwork.inRoom)
        {
            //Check the player count of the room.
            //When there are two players, we can start the game.
            if (PhotonNetwork.room.playerCount == 2)
            {
                //Close the room.
                PhotonNetwork.room.open = false;

                //Initialise the players in the playermanager.
                PlayerManager.Initialise();

                //Display the connecting overlay.
                overlayController.ShowOverlay(2);

                //Load the game from the master client and scene syncing will carry any other clients along.
                //Wait a second to allow the players to initialise.
                if (PhotonNetwork.isMasterClient)
                    Invoke("OpenSetUpScene", 0.5f);
            }
        }
    }

    //Called once the user connects to a lobby
    void OnJoinedLobby()
    {
        //Update the photon text and the UI.
        connectionText.text = "CONNECTED TO LOBBY";

        //Show the menu
        menuPanel.SetActive(true);

        //Hide the waiting overlay when the player returns to the lobby.
        overlayController.HideAll();
    }

    //Called after successfully joining a room.
    //Managing the cancel button within this method is easier than using Unity's UnityEvents system.
    void OnJoinedRoom()
    {
        //Update connection text.
        connectionText.text = "CONNECTED TO GAME ROOM";

        //display overlay
        //waitingOverlay.SetActive(true);
        overlayController.ShowOverlay(0);

        //Set up the cancel button.
        Button cancelButton = GameObject.Find("CancelButton").GetComponent<Button>();
        cancelButton.interactable = true;
        cancelButton.onClick.AddListener(() =>
        {
            //Disable the button.
            cancelButton.interactable = false;

            //Disconnect from the room.
            PhotonNetwork.LeaveRoom();
        });
    }

    //This method is called when a player tries to join a room which is inacessable.
    //This will present the warning overlay to the player and return them to the lobby.
    void OnPhotonJoinRoomFailed()
    {
        //Show warning overlay.
        overlayController.ShowOverlay(3);
    }

    //When disconnecting from photon, reattempt to connect.
    //This should only be called when the user attempts to join but fails and retries
    void OnDisconnectedFromPhoton()
    {
        //Connect to the photon server.
        PhotonNetwork.ConnectUsingSettings(GameVersion);
    }

    //Launches the setup scene.  This is called in an invoke method to start after a second.
    //This allows extra time for the game to initialise and prevents desync errors.
    void OpenSetUpScene()
    {
        PhotonNetwork.LoadLevel(1);
    }

    //The play button on the menu panel.
    public void PlayGameButton()
    {
        //When this button is pressed, we want to join or create a room and place the player in it. 
        //When the game is then launched on the other platform, that player will join the room.
        connectionText.text = "CONNECTING TO GAME ROOM";
        RoomOptions options = new RoomOptions();
        options.maxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom("Session", options, TypedLobby.Default);
    }

    //The exit button on the menu panel.
    public void ExitApplicationButton()
    {
        //Close the application.
        Application.Quit();
    }

    //The how to play button on the menu panel.
    public void ShowInstructionsButton()
    {
        //Show the instructions overlay.
        overlayController.ShowOverlay(1);
    }

    //The close button of the instructions overlay.
    public void CloseOverlayButton()
    {
        //Hide all overlays.
        overlayController.HideAll();
    }
}
