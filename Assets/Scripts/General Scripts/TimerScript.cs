using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    //the method to be called when the timer expires.
    public UnityEvent timerExpiredCallback;

    //The text field to write the time to.
    Text timerText;

    //The amount of time on the timer.
    public int timerInterval;

    //This retrieves the timer value.
    static int timer;

    //This returns whether the timer is currently ticking.
    public bool isRunning
    {
        get
        {
            return IsInvoking("Tick");
        }
    }


    public void Start()
    {
        //Initialise the startime.
        timer = timerInterval;
        timerText = GameObject.Find("TimeTextTarget").GetComponent<Text>();

        //Update the time text.
        timerText.text = timer.ToString() + " seconds.";

        //Start the timer.
        StartTimer();
    }

    //This method starts the timer to tick every second.
    public void StartTimer()
    {
        InvokeRepeating("Tick", 1, 1);
    }

    //This method stops the timer and optionally restarts it.
    public void StopTimer(bool resetTime)
    {
        CancelInvoke("Tick");

        if (resetTime)
        {
            //Reset the timer.
            timer = timerInterval;

            //Start the timer.
            StartTimer();
        }
    }

    //If the timer ticks out, then invoke the method.
    //otherwise, tick. 
    void Tick()
    {
        if (timer == 0)
        {
            //timer = -1;
            timerExpiredCallback.Invoke();
            print("Invoked");
        }
        else
        {
            timer--;
        }

        //Update the time text.
        timerText.text = timer.ToString() + " seconds.";
    }
}
