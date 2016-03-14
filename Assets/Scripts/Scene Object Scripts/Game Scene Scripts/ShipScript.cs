using UnityEngine;

//This script is attached to each ship and processes operations such as board movement and damage.
//This script is used during the gameplay phase, where the ships have no "input" role.
public class ShipScript : MonoBehaviour
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
        currentHealth--;
    }

    //Reveals the ship.
    public void Reveal()
    {
        sRenderer.enabled = true;
    }
}