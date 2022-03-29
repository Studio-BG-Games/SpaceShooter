using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HelpLinks : EditorWindow
{
    [MenuItem("Space Combat Kit/Help/Tutorial Videos")]
    public static void TutorialVideos()
    {
        Application.OpenURL("https://vimeo.com/user102278338");
    }

    [MenuItem("Space Combat Kit/Help/Forum")]
    public static void Forum()
    {
        Application.OpenURL("https://forum.unity.com/threads/space-combat-kit-vsxgames-released.340962/");
    }
}
