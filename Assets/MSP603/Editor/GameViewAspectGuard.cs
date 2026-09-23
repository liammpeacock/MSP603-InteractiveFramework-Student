using UnityEditor;
using UnityEngine;

namespace MSP603.TowerDefense.Editor
{
    /// <summary>
    /// Warns students when the Game view can misalign world objects with the painted level backdrop.
    /// This Editor-only check does not change the Game view or runtime camera.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewAspectGuard
    {
        public const float TargetAspect = 16f / 9f;
        public const float AspectTolerance = 0.02f;

        private const string DialogTitle = "MSP603 Game View: Use 16:9";
        private const string DialogMessage =
            "MSP603 levels are designed for a 16:9 Game view.\n\n" +
            "Your current Game view can make towers look offset from their placement pads. " +
            "Nothing is necessarily wrong with tower placement.\n\n" +
            "In the Game view toolbar, open the aspect-ratio menu (it may say Free Aspect), " +
            "then select 16:9 to see the level as intended.";

        private static bool warningShownThisPlaySession;

        static GameViewAspectGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static bool IsApproximatelySixteenByNine(float width, float height)
        {
            if (width <= 0f || height <= 0f)
            {
                return false;
            }

            return Mathf.Abs(width / height - TargetAspect) <= AspectTolerance;
        }

        public static bool ShouldWarnForSession(float width, float height, ref bool warningAlreadyShown)
        {
            if (warningAlreadyShown || width <= 0f || height <= 0f ||
                IsApproximatelySixteenByNine(width, height))
            {
                return false;
            }

            warningAlreadyShown = true;
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                warningShownThisPlaySession = false;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall += CheckGameViewAspect;
            }
        }

        private static void CheckGameViewAspect()
        {
            if (Application.isBatchMode || !EditorApplication.isPlaying)
            {
                return;
            }

            Vector2 gameViewSize = Handles.GetMainGameViewSize();
            if (ShouldWarnForSession(gameViewSize.x, gameViewSize.y, ref warningShownThisPlaySession))
            {
                EditorUtility.DisplayDialog(DialogTitle, DialogMessage, "Got it");
            }
        }
    }
}
