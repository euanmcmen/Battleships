using UnityEngine;

//This script is attached and used during the setup phase.
//It contains values and behaviours to allow the ship identify the board and recieve input events pertaining to setup.
public class ShipSetupScript : MonoBehaviour
{
    //The containing bounding box of the board.
    Bounds boardBox;

    //The ship's bounding box.
    Bounds shipBounds;

    //The sprite renderer attached to the ship.
    SpriteRenderer sRenderer;

    //This is the offset of the ship, in case it doesn't rotate easily around the center point.
    //The only offset in the game at the moment is the small ship which is 0.5 units off.
    public float pivotOffset;

    //The sprite layer sorting order value to reset when the ship is deselected.
    int defaultSortingOrder;

    //Property to encapsulate the pivot return.  Whenever the pivot is requested, the updated point will be returned.
    public Vector2 pivot
    {
        get
        {
            return transform.position + (transform.up * pivotOffset);
        }
    }

    //A value to represent if the ship is selected.
    public bool isSelected = false;

    void Start()
    {
        //Initialise the sprite renderer
        sRenderer = GetComponent<SpriteRenderer>();
        defaultSortingOrder = sRenderer.sortingOrder;

        //Locate the bounding boxes.
        boardBox = GameObject.Find("BoardBox").GetComponent<Collider>().bounds;
        shipBounds = GetComponent<Collider>().bounds;        
    }

    //Rotates the ship on the board.
    public void RotateShip()
    {
        //Unlike the moveship method, we can't simply rotate the bounds of the ship.  Instead, we rotate the ship regardless, and rotate it back if the collision check fails.
        //Store the old rotation and position.
        Quaternion oldRot = transform.rotation;
        Vector3 oldPos = transform.position;

        //Rotate the ship 90 degrees on the z axis around the pivot point.
        transform.RotateAround(pivot, Vector3.back, 90);

        //If the rotation is equal to 360, set it to zero.
        if (transform.rotation.eulerAngles.z == 360)
            transform.rotation = Quaternion.identity;

        //Refresh the bounding volume to fit the object after rotation.
        shipBounds = GetComponent<Collider>().bounds;

        //Validate rotation
        if (!ValidateShipMovement(shipBounds))
        {
            //If the rotation fails, reset the values.
            transform.rotation = oldRot;
            transform.position = oldPos;
            shipBounds = GetComponent<Collider>().bounds;
        }
    }

    //Moves the ship on the board.
    public void MoveShip(Vector2 direction)
    {
        //Set the new position of the ship to where the ship will end up after a successful move.
        Vector3 newPos = new Vector3(transform.position.x + direction.x, transform.position.y + direction.y, 0);

        //Move the ship bounds to the new position.
        shipBounds.center = newPos;

        //Validate movement with the collision check method.
        if (ValidateShipMovement(shipBounds))
        {
            //Move the ship to the new position after a successful validation.
            shipBounds.center = transform.position;
            transform.position = newPos;
        }
        else
        {
            //Move the bounds back to the ship after a failed validation.
            shipBounds.center = transform.position;
        }
    }

    //This method confirms that the ship is moving to a legal position.
    bool ValidateShipMovement(Bounds newBounds)
    {
        //Check if the ship is still within the box.
        if (newBounds.ContainedIn(boardBox))
        {
            foreach (Bounds b in ShipManager.GetUnselectedShipBounds())
            {
                if (newBounds.Intersects(b))
                    return false;
            }

            //After successfully looping though each ship, and being contained, return true.
            return true;
        }
        else
            return false;
    }

    public void SelectShip()
    {
        //Set the ship's selected value.
        isSelected = true;

        //Bring the ship to the front of the sprite draw order.
        sRenderer.sortingOrder = 5;
    }

    public void DeselectShip()
    {
        //Set the ship's selected value.
        isSelected = false;

        //Send the ship back in the sprite draw order.
        sRenderer.sortingOrder = defaultSortingOrder;
    }
}
