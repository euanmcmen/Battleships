using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BatchBuildScript
{
    //Identify scenes.
    static string[] scenes = new string[3] { "Assets/Scenes/MenuScene.unity", "Assets/Scenes/SetupScene.unity", "Assets/Scenes/GameScene.unity" };

    //The priority number reflects where in the custom menu the function is placed.
    //The numbering convention is xy wheere x is the "sction", seperated by a seperator, and y is the place in the whole menu.
    //There must be a difference of at least 10 between the previous section last function, and the new section first function.
    [@MenuItem("Custom/Batch Build",false, 01)]
    static void BatchBuild()
    {
        //Disable game objects.
        DisableGameObjects();

        //Build to Windows device.
        BuildPipeline.BuildPlayer(scenes, "Builds/Windows/Battleships.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);

        //Build to android.
        BuildPipeline.BuildPlayer(scenes, "Builds/Android/Battleships.apk", BuildTarget.Android, BuildOptions.AutoRunPlayer);
    }

    [@MenuItem("Custom/Build To Windows", false, 12)]
    static void BuildWindows()
    {
        //Disable game objects.
        DisableGameObjects();

        //Build to Windows device.
        BuildPipeline.BuildPlayer(scenes, "Builds/Windows/Battleships.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }

    [@MenuItem("Custom/Build To Android", false, 13)]
    static void BuildAndroid()
    {
        //Disable game objects.
        DisableGameObjects();

        //Build to android.
        BuildPipeline.BuildPlayer(scenes, "Builds/Android/Battleships.apk", BuildTarget.Android, BuildOptions.AutoRunPlayer);
    }

    [@MenuItem("Custom/Switch To Windows", false, 24)]
    static void SwitchToWindowsPlatform()
    {
        //Change to windows build target
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
    }

    [@MenuItem("Custom/Switch To Android", false, 25)]
    static void SwitchToAndroidPlatform()
    {
        //Change to windows build target
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
    }

    static void DisableGameObjects()
    {
        //Get all loaded scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            //If the scene is loaded, then go through the objects at the root.
            //Disable the objects marked as disableonbuild.
            if (SceneManager.GetSceneAt(i).isLoaded)
            {
                //Hold a reference to the scene.
                Scene scene = SceneManager.GetSceneAt(i);

                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go.tag == "DisableOnBuild")
                    {
                        go.SetActive(false);
                    }
                }

                //Save the scene.
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
