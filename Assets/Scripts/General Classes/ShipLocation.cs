using UnityEngine;

//This class holds a single collection of position and rotation values for a ship.
public class ShipLocation
{
    //The values for the ship's presence on the board.
    public Vector2 position;
    public ShipOrientation rotation;

    //The size of this object in bytes.
    //Vector2s consist of 2 floats, and the shiporientation enum uses integers.
    public static int size = (sizeof(float) * 2) + sizeof(int);

    //A constructor which initialises the values.
    public ShipLocation(Vector2 pos, ShipOrientation rot)
    {
        position = pos;
        rotation = rot;
    }
}
