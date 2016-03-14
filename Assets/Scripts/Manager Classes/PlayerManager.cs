//This class provides a consistant reference to the player objects thoughout the game.
//Using this class avoids the need to create donotdestroyonload objects to move the player scripts between scenes.
public static class PlayerManager
{
    //This is the local reference to the player with the id of 1.
    static Player player1;

    //This is the local reference to the player with the id of 2.
    static Player player2;

    public static void Initialise()
    {
        //Assign the players by retrieving the ids of the photon players in the room.
        player1 = new Player(PhotonNetwork.player.Get(1), "Player 1");
        player2 = new Player(PhotonNetwork.player.Get(2), "Player 2");
    }

    //Returns the local player as a player object.
    public static Player GetLocalPlayer()
    {
        //If player 1 is the local player, return player 1.
        return (player1.GetPhotonPlayer().isLocal) ? player1 : player2;
    }

    //Returns the other player as a player object.
    public static Player GetOtherPlayer()
    {
        //If player 1 is the local player, return player 2.
        return (player1.GetPhotonPlayer().isLocal) ? player2 : player1;
    }
}
