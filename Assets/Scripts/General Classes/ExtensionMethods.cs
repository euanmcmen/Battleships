using UnityEngine;

public static class ExtensionMethods
{
    //Create a containment method for the Bounds class.
    //the "this" keyword in the parameters indicates that this will be an extension method of the Bounds class and the parameter will be the "bigger box".
    public static bool ContainedIn(this Bounds smallBox, Bounds biggerBox)
    {
        return ((smallBox.min.x >= biggerBox.min.x
            && smallBox.max.y <= biggerBox.max.y
            && smallBox.max.x <= biggerBox.max.x
            && smallBox.min.y >= biggerBox.min.y));
    }
}
