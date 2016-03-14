using UnityEngine;

//This script is used during the gameplay state to control the tiles and the linked ships.
public class TileScript : MonoBehaviour
{
    //Colours for the tile and peg combinations for this script.
    //- Default [BLACK] - The default state for the tile
    //- Selected [YELLOW] - The tile has been selected as one of the hit targets for the user's turn.
    //- Hit [RED] - The tile contains an enemy ship and is marked as a hit. 
    //- Miss [WHITE] - The tile did not contain an enemy ship and is marked as a miss.
    SpriteRenderer tileSRenderer;

    //The shipscript attached to the ship ontop of the tile, if one exists.
    ShipScript ship = null;

    //If the tile was previously selected and was fired upon, it is hit.
    //Tiles in this state should not be selectable.
    bool isHit = false;

    //if a tile is selected but has not been hit, it is selected.  
    //This state can be reverted before the firing stage.
    bool isSelected = false;

    //A tile is only selectable if it is not already selected and has not been fired upon and is n
    public bool isSelectable
    {
        get
        {
            return !isSelected && !isHit; 
        }
    }

    //A tile is only deselctable if it is selected and has not been fired upon.
    public bool isDeselectable
    {
        get
        {
            return isSelected && !isHit;
        }
    }

    // Use this for initialization
    void Start()
    {
        //Set compnents by accessing children.
        tileSRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        //Assign default colours.
        tileSRenderer.color = Color.black;
    }

    //This selects the tile and sets the tile to unselectable.
    public void SelectTile()
    {
        //Set the tile to selected.
        isSelected = true;

        //Set the tile colour.
        tileSRenderer.color = Color.yellow;
    }

    //This returns the tile to the default state and unselects it.
    public void DeselectTile()
    {
        //Set the tile to unselected.
        isSelected = false;

        //Set the tile colour.
        tileSRenderer.color = Color.black;
    }

    //This method checks if a ship is contained on the tile.
    //The referenced ship can be used for post-processing by the gamecontrolscript.
    public bool CheckHit(out ShipScript hit)
    {
        //Initialise the result to be used for tile colouring.
        bool result = false;

        //Check if there is a ship on the tile.
        if (ship != null)
        {
            //Assign the ship to the hitShip for post-processing
            hit = ship;

            //set the result to true
            result = true;            
        }
        else
        {
            //Ship hits nothing so ship script is false.
            hit = null;

            //Set result to false.
            result = false;
        }

        //Set the tile colour depending on whether the shot hit or not.
        tileSRenderer.color = result ? Color.red : Color.white;

        //Set the tile to be hit and prevent further shots.
        isHit = true;

        //Return the result value.
        return result;
    }

    //When the ships are created, they will collide with the tile.  
    //Assign the tile a ship reference in this case.
    void OnCollisionEnter(Collision col)
    {
        //Assign ship to tile.
        if (col.gameObject.tag == "Ship")
        {
            //Assign the ship to the tile.
            ship = col.gameObject.GetComponent<ShipScript>();
        }
    }
}
