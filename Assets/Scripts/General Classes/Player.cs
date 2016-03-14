//This represents an instance of a player in the game.
//Each player has an id and name to identify them.
//The player also has a fleet.
public class Player
{
    //The player's id.
    public readonly int ID;

    //The player's name.
    public readonly string Name;
    
    //The player's fleet positions.
    public Fleet fleet = null;

    //The player's "ready state" for a phase sync point.
    //This occurs before a transition another scene to make sure both players are at the same point.
    public bool isReady = false;

    //A value to determine if the player is the active player.
    public bool isActivePlayer = false;

    //Constructor for the player taking in a photon player.
    public Player(PhotonPlayer player, string name)
    {
        ID = player.ID;
        Name = name;
    }

    //Returns the photon player sharing an id with this player.
    public PhotonPlayer GetPhotonPlayer()
    {
        return PhotonPlayer.Find(ID);
    }

    //Returns a string representation of the player.
    public override string ToString()
    {
        return string.Format("Name: {0} | ID: {1} | isLocal: {2} | Has Fleet: {3}", Name, ID, GetPhotonPlayer().isLocal, (fleet != null));
    }
}
