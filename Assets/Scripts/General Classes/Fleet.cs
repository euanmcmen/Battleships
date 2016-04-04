using UnityEngine;
using ExitGames.Client.Photon;

//This class contains a set of datapair classes, each with information pertaining to ship spawning.
//The fleet class makes up the position of the ships on the board.
public class Fleet
{
    //The ship positions
    public ShipLocation smallShip;
    public ShipLocation mediumShip;
    public ShipLocation largeShip;

    //The size of this object in bytes.
    //ShipLocation size: (sizeof(float) * 2) + sizeof(int). 
    //Fleet is comprised of 3 shiplocations.
    public static int size = ShipLocation.size * 3;

    //Constuctor for the fleet.
    public Fleet(ShipLocation small, ShipLocation medium, ShipLocation large)
    {
        //Sets the ships.
        smallShip = small;
        mediumShip = medium;
        largeShip = large;
    }

    //Gives a text representation of all the ships in the fleet.
    public override string ToString()
    {
        return string.Format("Small Ship [ Position: {0}, Rotation: {1} ]; Medium Ship [ Position: {2}, Rotation: {3} ]; Large Ship [ Position: {4}, Rotation: {5} ];", 
            smallShip.position, smallShip.rotation, mediumShip.position, mediumShip.rotation, largeShip.position, largeShip.rotation);
    }

    //Serializes the fleet for use with Photon serialization.
    //Must have a byte[] return and take in an object type.
    public static byte[] Serialize(object obj)
    {
        //Cast the object to a fleet object
        Fleet fleet = (Fleet)obj;

        //Get the ship locations from the fleet.
        ShipLocation[] ships = fleet.GetShipsArray();

        //Set up the byte array and byte index for the serialization process.
        byte[] bytes = new byte[size];
        int byteIndex = 0;

        //Add the positions to the byte array.  The position is a vector 2, which is comprised of two floats.
        //Then add the rotation into the byte array.  The position is just an enum with an integer value.
        //Do this for each ship in the fleet.
        foreach (ShipLocation sl in ships)
        {
            Protocol.Serialize(sl.position.x, bytes, ref byteIndex);
            Protocol.Serialize(sl.position.y, bytes, ref byteIndex);
            Protocol.Serialize((int)sl.rotation, bytes, ref byteIndex);
        }

        return bytes;
    }

    //Deserialize method for photon.
    //Must take in a byte[] and output a Fleet type.
    public static Fleet Deserialize(byte[] bytes)
    {
        //Initialise the objects to retrieve from the byte array.
        Vector2[] locations = new Vector2[3];
        int[] rotations = new int[3];
        ShipLocation[] pairs = new ShipLocation[3];

        //The index which will mark the position in the byte array.
        int byteIndex = 0;

        //Retrieve the position from the array.  
        //The order of the array depends on how it was serialized, so the order is: pos.x, pos.y, rot
        //Build each ship this way.
        for (int i = 0; i < 3; i++)
        {
            locations[i] = new Vector2();
            Protocol.Deserialize(out locations[i].x, bytes, ref byteIndex);
            Protocol.Deserialize(out locations[i].y, bytes, ref byteIndex);
            Protocol.Deserialize(out rotations[i], bytes, ref byteIndex);
            pairs[i] = new ShipLocation(locations[i], (ShipOrientation)rotations[i]);
        }

        //Return the fleet from the pairs array.
        return new Fleet(pairs[0], pairs[1], pairs[2]);
    }

    //Returns the ships of a fleet as an array of shiplocations for enumaration.
    public ShipLocation[] GetShipsArray()
    {
        //Adds each ship to an array and returns the array.
        ShipLocation[] ships = new ShipLocation[3];
        ships[0] = smallShip;
        ships[1] = mediumShip;
        ships[2] = largeShip;
        return ships;
    }
}
