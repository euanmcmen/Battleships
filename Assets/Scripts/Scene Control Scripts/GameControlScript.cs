using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameControlScript : Photon.MonoBehaviour
{
    //Icons for the ui.  These show the destoryed ship icons in the status panels.
    public Sprite smallShipDestroyedIcon;
    public Sprite mediumShipDestroyedIcon;
    public Sprite largeShipDestroyedIcon;

    //The local and other players from the perspective of this instance.
    //These will reference the player objects in the playermanager.
    Player localPlayer = null;
    Player otherPlayer = null;

    //Text UI elements
    Text smallShipPanelText;
    Text mediumShipPanelText;
    Text largeShipPanelText;
    Text shotCountText;
    Text turnText;

    //The ship overlay components
    GameObject smallShipPanel;
    GameObject mediumShipPanel;
    GameObject largeShipPanel;

    //The fire button
    Button fireButton;

    //The firing list.  
    List<TileScript> selectedTiles = new List<TileScript>(3);

    //Arrays of the ships.
    GameObject[] shipObjects;
    ShipScript[] shipScripts;

    //The amount of shots remaimning on this turn.
    int shotsRemaining = 0;

    //The total amount of shots available to the player this turn.
    //This is equal to the number of active ships on the board
    int maxShots = 0;

    //This is the control script for the overlays.
    OverlayScript overlayController;

    //The timer controller attached to this object
    TimerScript timerController;

    //value to determine whether the UI should be updated.
    bool shouldUpdateUI = false;

    // Use this for initialization
    void Start()
    {
        //Get the other components of the object.
        overlayController = GetComponent<OverlayScript>();
        timerController = GetComponent<TimerScript>();

        //Get the players.
        localPlayer = PlayerManager.GetLocalPlayer();
        otherPlayer = PlayerManager.GetOtherPlayer();

        //Add ships to the list.
        //Spawn the ships from the other player's fleet using the game ship resources.  These are invisible and use the destroyed sprite.
        shipObjects = new GameObject[3];
        shipObjects[0] = (Instantiate(Resources.Load<GameObject>("Game/Ships/SmallShip"), otherPlayer.fleet.smallShip.position, Quaternion.Euler(0, 0, (int)otherPlayer.fleet.smallShip.rotation)) as GameObject);
        shipObjects[1] = (Instantiate(Resources.Load<GameObject>("Game/Ships/MediumShip"), otherPlayer.fleet.mediumShip.position, Quaternion.Euler(0, 0, (int)otherPlayer.fleet.mediumShip.rotation)) as GameObject);
        shipObjects[2] = (Instantiate(Resources.Load<GameObject>("Game/Ships/LargeShip"), otherPlayer.fleet.largeShip.position, Quaternion.Euler(0, 0, (int)otherPlayer.fleet.largeShip.rotation)) as GameObject);

        //Get the scripts attached to the ships.
        shipScripts = new ShipScript[3];
        for (int i = 0; i < shipObjects.Length; i++)
        {
            shipScripts[i] = shipObjects[i].GetComponent<ShipScript>();
        }

        //Set up the ship info panels for the local player.
        smallShipPanel = GameObject.Find("SmallShipHealthPanel");
        mediumShipPanel = GameObject.Find("MediumShipHealthPanel");
        largeShipPanel = GameObject.Find("LargeShipHealthPanel");

        //Get the text components for the text objects.
        smallShipPanelText = smallShipPanel.GetComponentInChildren<Text>();
        mediumShipPanelText = mediumShipPanel.GetComponentInChildren<Text>();
        largeShipPanelText = largeShipPanel.GetComponentInChildren<Text>();
        shotCountText = GameObject.Find("ShotsCount").GetComponent<Text>();
        turnText = GameObject.Find("TurnText").GetComponent<Text>();

        //Set up the fire button.
        fireButton = GameObject.Find("FireButton").GetComponent<Button>();
        fireButton.onClick.AddListener(() =>
        {
            for (int i = 0; i < selectedTiles.Count; i++)
            {
                //Initialise a ship script to be initialised by the checkhit method.
                ShipScript ss = null;

                //Returning true implies that the shipscript will be initialised as checkhit will only return true when ship is not null.
                if (selectedTiles[i].CheckHit(out ss))
                {
                    //Send an rpc call to damage the hit shipscript.
                    //Send the ship's name for serlization.  It will be redetected on "the other side"
                    photonView.RPC("HitShip", otherPlayer.GetPhotonPlayer(), ss.name);
                }

                //Update the ui after each shot.
                shouldUpdateUI = true;
            }

            //Tell both players to switch active players.
            photonView.RPC("EndTurn", PhotonTargets.All);
        });

        //Set the master client to make the first move, as dictated by the laws of Host Advantage
        if (PhotonNetwork.isMasterClient)
            SetLocalPlayerAsActivePlayer();
        else
            SetOpponentPlayerAsActivePlayer();

        //Initialise the shots available.
        DetermineShots();

        //Initialise the UI elements
        UpdateUIElements();
    }

    // Update is called once per frame
    void Update()
    {
        //Only allow input if the local player is the active player.
        if (localPlayer.isActivePlayer)
        {

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            //Check for pc input
            //Make sure the user is selecting a game object which does not use the event system.
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(-1))
            {
                ProcessInputEvent();
            }
#endif


#if UNITY_ANDROID
                //Android controls
                //If it's the local player's turn and there's an touch, fire a ray from the position to see if it hit a tile.
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(0))
                    {
                        ProcessInputEvent();
                    }
                }
#endif

        }

        //Update the ui elements if necessary.
        if (shouldUpdateUI)
            UpdateUIElements();


        //Quit when the back button or escape is pressed.
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }

    //This method processes input events and carries out the tile check and processing operations.
    void ProcessInputEvent()
    {
        //Initialise the raycast.
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Cast the ray and check if it hits a tile.
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.tag == "Tile")
            {
                //Set the tilescript value.
                TileScript ts = hit.collider.GetComponent<TileScript>();

                //Set the tile is selectable and we have a shot, and add it to the list.
                if (ts.isSelectable && shotsRemaining > 0)
                {
                    //Decrement shots and update the shots ui.
                    shotsRemaining--;

                    //Add the tile to the list
                    selectedTiles.Add(ts);

                    //Select the tile.
                    ts.SelectTile();
                }

                //If the tile is deselectable, then unselect it and remove it from the list.
                else if (ts.isDeselectable && shotsRemaining < maxShots)
                {
                    //Increment the shots and update the ui
                    shotsRemaining++;

                    //remove the tile from the list
                    selectedTiles.Remove(ts);

                    //Deselct the tile.
                    ts.DeselectTile();
                }
            }
        }

        //Update the ui after touching a tile
        shouldUpdateUI = true;
    }

    void DetermineShots()
    {
        //Clear the active ships list
        List<ShipScript> activeShips = new List<ShipScript>(3);

        //Loop through all ship scripts and add them to the list.
        foreach (ShipScript ss in shipScripts)
        {
            if (ss.isAlive)
            {
                activeShips.Add(ss);
            }
        }

        //Get the max shots from the length of the ships array.
        maxShots = activeShips.Count;
        shotsRemaining = maxShots;
    }

    void UpdateUIElements()
    {
        //Write the ship health to the health panels.
        smallShipPanelText.text = shipScripts[0].GetHealthString();
        mediumShipPanelText.text = shipScripts[1].GetHealthString();
        largeShipPanelText.text = shipScripts[2].GetHealthString();

        //Check if ships took a hit.  If so, play the blink animation.
        if (shipScripts[0].tookHit)
            smallShipPanel.GetComponent<Animator>().Play("Blink");

        if (shipScripts[1].tookHit)
            mediumShipPanel.GetComponent<Animator>().Play("Blink");

        if (shipScripts[2].tookHit)
            largeShipPanel.GetComponent<Animator>().Play("Blink");

        //Check for the alive status of each ship and update the status panels and board if necessary.
        if (!shipScripts[0].isAlive)
            smallShipPanel.transform.GetChild(0).GetComponent<Image>().sprite = smallShipDestroyedIcon;

        if (!shipScripts[1].isAlive)
            mediumShipPanel.transform.GetChild(0).GetComponent<Image>().sprite = mediumShipDestroyedIcon;

        if (!shipScripts[2].isAlive)
            largeShipPanel.transform.GetChild(0).GetComponent<Image>().sprite = largeShipDestroyedIcon;

        //Update the shots string to be current shots by max shots.
        shotCountText.text = "Shots: " + shotsRemaining + " / " + maxShots;

        //If the user has selected all tiles, then enable the button.
        fireButton.interactable = (selectedTiles.Count == maxShots);

        //Update complete
        shouldUpdateUI = false;
    }


    //This method sets a variable which will force the game to switch active players.
    //This is called from the timer control script.
    public void ForceEndTurn()
    {
        //Deselect any selected tiles.
        foreach (TileScript ts in selectedTiles)
        {
            //Deselct the tile.
            ts.DeselectTile();
        }

        EndTurn();
    }

    //This sets the local player as active player, and updates the turn panel respectively.
    void SetLocalPlayerAsActivePlayer()
    {
        localPlayer.isActivePlayer = true;
        otherPlayer.isActivePlayer = false;
        turnText.text = "Your Turn";
        turnText.transform.parent.GetComponent<Image>().color = Color.green;
    }

    //This sets the opponent player as active player, and updates the turn panel respectively.
    void SetOpponentPlayerAsActivePlayer()
    {
        localPlayer.isActivePlayer = false;
        otherPlayer.isActivePlayer = true;
        turnText.text = "Opponent Turn";
        turnText.transform.parent.GetComponent<Image>().color = Color.red;
    }

    //This method will be called on both clients and will carry out any endturn evaluations such as victory and set the new active player.
    [PunRPC]
    void EndTurn()
    {
        //Reset the timer.
        timerController.StopTimer(true);

        //Clear the selected tiles list.
        selectedTiles.Clear();

        //Initialise the shots available to both players after this turn.
        DetermineShots();

        //If the local player is the active player, then set the other's active status to true and local's to false.
        //Otherwise, if the local player is not the active player, set local to true and other to false.
        if (localPlayer.isActivePlayer)
        {
            SetOpponentPlayerAsActivePlayer();
        }
        else
        {
            SetLocalPlayerAsActivePlayer();
        }

        //Check if the local player has been defeated by checking how many shots he has available.
        if (maxShots == 0)
        {
            //Stop the timer.
            timerController.StopTimer(false);

            //Show defeat panel.
            overlayController.ShowOverlay(1);

            //Tell the other client to show the victory panel.
            photonView.RPC("ShowVictoryOverlay", otherPlayer.GetPhotonPlayer());
        }

        //Update the ui.
        shouldUpdateUI = true;
    }

    //This is called on all platforms to determine the winner.
    //An overlay will be shown depending on the outcome of the game.
    [PunRPC]
    void ShowVictoryOverlay()
    {
        //Stop the timer on the other platform.
        timerController.StopTimer(false);

        //Show victory panel
        overlayController.ShowOverlay(0);
    }

    //This method is called on the other player's client and damages a ship passed in though the shipscript.
    [PunRPC]
    void HitShip(string shipName)
    {
        //Get the ship from the name.
        ShipScript ss = GameObject.Find(shipName).GetComponent<ShipScript>();

        //and tell it to take damage.
        ss.TakeDamage();

        //Check if the ship is alive.  If not, then tell the other client to reveal the ship.
        if (!ss.isAlive)
        {
            photonView.RPC("RevealShip", otherPlayer.GetPhotonPlayer(), shipName);
        }
    }

    //This method is called from the other player on the local player's client to reveal a ship if it sustains fatal damage.
    [PunRPC]
    void RevealShip(string shipName)
    {
        //Reveal the ship.
        GameObject.Find(shipName).GetComponent<ShipScript>().Reveal();
    }

    //This is called when a user presses the return to menu ui button.
    //It presents the user with an overlay which will allow them to disconnect.
    public void MenuUIButton()
    {
        //Show return to menu panel
        overlayController.ShowOverlay(3);
    }

    //Returns the user to the menu and disconnects them from photon.
    //This is called when a return to menu button is pressed or the user is disconnected and presses the overlay button.
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
