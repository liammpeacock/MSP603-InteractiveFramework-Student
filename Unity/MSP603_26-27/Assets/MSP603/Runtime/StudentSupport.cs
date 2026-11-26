using UnityEngine;

namespace MSP603.TowerDefense
{
    /// <summary>Central configuration for student support links supplied with the framework.</summary>
    public static class StudentSupport
    {
        public const string BugReportUrl = "https://forms.cloud.microsoft/Pages/ResponsePage.aspx?id=h3BeEEd8jEGXmpueC7A9vCLsXv-7TCBAlLsMG1rOBb1UQ0FETTdBVDgwQTRON0UyQlRIMkpJMDMzSi4u";

        public static void OpenBugReport()
        {
            Application.OpenURL(BugReportUrl);
        }
    }
}
