using UnityEngine;

//This class contains methods and fields to control all ships.
//The main task for this class is to expose the ship objects for each ship instance to loop though while carrying out movement checks.
public static class ShipManager
{
    //The array of ships for use in the bounds check algorithm.
    static GameObject[] ships;

    //This method returns a Fleet layout to spawn the ships.
    public static Fleet GetSpawnPositions()
    {
        //Set up the spawns list.
        Fleet[] spawnsList = new Fleet[3];

        //Initialise an instance of the a datapair to use in the placement of the ships.
        //Fill the lists with data.
        spawnsList[0] = new Fleet
        (
            new ShipLocation(new Vector2(3, -2.5f), ShipOrientation.DOWN),        //Small
            new ShipLocation(new Vector2(-2, 3), ShipOrientation.RIGHT),          //Medium
            new ShipLocation(new Vector2(-2, -1), ShipOrientation.UP)             //Large
        );

        spawnsList[1] = new Fleet
        (
            new ShipLocation(new Vector2(-2.5f, -3), ShipOrientation.RIGHT),      //Small
            new ShipLocation(new Vector2(-2, 0), ShipOrientation.RIGHT),          //Medium
            new ShipLocation(new Vector2(-1, 2), ShipOrientation.RIGHT)           //Large
        );

        spawnsList[2] = new Fleet
        (
            new ShipLocation(new Vector2(2.5f, -3), ShipOrientation.LEFT),        //Small
            new ShipLocation(new Vector2(2, 0), ShipOrientation.LEFT),            //Medium
            new ShipLocation(new Vector2(1, 2), ShipOrientation.LEFT)             //large
        );

        //Get a random spawn setup.
        int rndIndex = Random.Range(0, spawnsList.Length);
        return spawnsList[rndIndex];
    }

    //This is written to from the control script to allow the ships to be enumarated by other classes - specifically the shipsetup class.
    public static void WriteShipsToManager(GameObject[] shipObjects)
    {
        //Set up the ships array.
        ships = new GameObject[3];
        for (int i = 0; i < shipObjects.Length; i++)
        {
            ships[i] = shipObjects[i];
        }
    }

    //This method retrieves a list of the unselected ships' bounds.
    //This is a very specific method for the ship script's move function.
    public static Bounds[] GetUnselectedShipBounds()
    {
        //Initialise the array
        Bounds[] shipBounds = new Bounds[2];
        int boundsIndex = 0;

        //Fill the array
        foreach (GameObject go in ships)
        {
            //If this ship is not selected, then add its bounds to the bounds array and increase the index.
            if (!go.GetComponent<ShipSetupScript>().isSelected)
            {
                shipBounds[boundsIndex] = go.GetComponent<Collider>().bounds;
                boundsIndex++;
            }

            //Skip over the selected ship.
            else
            {
                continue;
            }
        }

        //Return the array.
        return shipBounds;
    }
}