using UnityEngine;

//This script is used by the setup scene to check for ship presense on a tile.
//Ship presence should be updated constantly though visual feedback.
public class TileSetupScript : MonoBehaviour
{
    //Colours for the tile and peg combinations for this script.
    //- Default [BLACK] - The default state for the tile.
    //- Occupied [GREEN] - The tile has a ship on top and is occupied.
    SpriteRenderer tileSRenderer;

    //The shipscript attached to the ship ontop of the tile, if one exists.
    //This will be detected using the pegs.
    ShipSetupScript ship = null;

    // Use this for initialization
    void Start()
    {
        //Set compnents by accessing children.
        tileSRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        //Assign default colours.
        tileSRenderer.color = Color.black;
    }

    public void OccupyTile(ShipSetupScript shipscript)
    {
        //Set the ship value to the ship sent by the beacon.
        ship = shipscript;

        //Set the tile to occupied - ternary operator.
        tileSRenderer.color = (ship.isSelected) ? Color.yellow : Color.green;
    }

    public void FreeTile()
    {
        //Rempve the ship value.
        ship = null;

        //If there's no ship on the tile, set it to default.
        tileSRenderer.color = Color.black;
    }

    void OnCollisionEnter(Collision col)
    {
        //Assign ship to tile.
        if (col.gameObject.tag == "Ship")
        {
            //Check if the tile does not have a ship and if not, assign one.
            if (ship == null)
                OccupyTile(col.gameObject.GetComponent<ShipSetupScript>());
        }
    }

    void OnCollisionStay(Collision col)
    {
        //When a ship is selected, the tile should change to yellow.
        if (ship != null)
            tileSRenderer.color = (ship.isSelected) ? Color.yellow : Color.green;
    }

    void OnCollisionExit(Collision col)
    {
        //Make sure a ship exists on the tile in order to remove it.
        if (ship != null)
        {
            if (col.gameObject.tag == "Ship")
            {
                //Check if the tile ship is the same as the colliding ship and free the tile if so.
                if (col.gameObject.name == ship.name)
                    FreeTile();
            }
        }
    }
}
