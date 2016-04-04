using UnityEngine;

public static class ExtensionMethods
{
    //Create a containment check method for the Bounds class.
    //The "this" keyword of the first parameter denotes this will be an extension method of the Bounds class.  
    //The second parameter is the parameter which will be passed in when calling this method.
    public static bool ContainedIn(this Bounds smallBox, Bounds biggerBox)
    {
        //Return true if the smaller box is contained within the bigger box.
        return ((smallBox.min.x >= biggerBox.min.x 
            && smallBox.max.y <= biggerBox.max.y
            && smallBox.max.x <= biggerBox.max.x
            && smallBox.min.y >= biggerBox.min.y));
    }
}
