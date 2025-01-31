using UnityEngine;

public class UserSession
{
     private static string userId;

    // Getter and Setter for userId
    public static string UserId
    {
        get { return userId; }
        set { userId = value; }
    }

    public static void ClearSession()
    {
        userId = null;
    }
}
