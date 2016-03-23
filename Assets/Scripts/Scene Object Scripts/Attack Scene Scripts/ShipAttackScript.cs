using UnityEngine;

//This script is attached to each ship and processes operations such as damage and ship death.
//This script is used during the attack phase, where the ships have no "input" role.
public class ShipAttackScript : MonoBehaviour
{
    //Reference to the ship's sprite renderer.
    SpriteRenderer sRenderer;

    //The max health of the ship.  This is also how many tiles the ship occupies.
    public int maxHealth;

    //The current or remaining health of the ship.
    public int currentHealth;

    //A value to represent whether the ship is alive.
    //A ship is alive when its health is above 0.
    public bool isAlive
    {
        get
        {
            return (currentHealth > 0);
        }
    }

    //Values to represent whether the ship took a hit in the current turn.
    private bool privateTookHit;
    public bool tookHit
    {
        get
        {
            //Only return true and reset if tookhit is true. 
            if (privateTookHit)
            {
                //This resets the value of tookhit to false before returning what was the value of tookhit.
                //This is only called when the field is accessed, thus "popping" the value.
                bool temp = privateTookHit;
                privateTookHit = false;
                return temp;
            }
            else
                return false;
        }
    }
    public void Start()
    {
        //Initialise the sprite renderer
        sRenderer = GetComponent<SpriteRenderer>();
    }

    //Returns the health of the ship as a string.
    public string GetHealthString()
    {
        return "Health: " + currentHealth + " / " + maxHealth;
    }

    //Takes a point off the ship's current health.
    public void TakeDamage()
    {
        //Subtract health
        currentHealth--;

        //Register a hit.
        privateTookHit = true;
    }

    //Reveals the ship.
    public void Reveal()
    {
        sRenderer.enabled = true;
    }
}