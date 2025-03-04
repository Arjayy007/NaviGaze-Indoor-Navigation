using System.Collections.Generic;
using UnityEngine;

public class NavigationHistoryData : MonoBehaviour
{
   public static Dictionary<string, object> ClassNavigation(string startingPoint, string destination, string navigationType, string status){
    
    return new Dictionary<string, object> {
        {"startingPoint", startingPoint},
        {"destination", destination},
        {"navigationType", navigationType},
        {"timestamp", System.DateTime.Now.ToString()},
        {"status", status}
    };
   }

   public static Dictionary<string, object> NormalNavigation (string startingPoint, string destination, string navigationType){
    
    return new Dictionary<string, object> {
        {"startingPoint", startingPoint},
        {"destination", destination},
        {"navigationType", navigationType},
        {"timestamp", System.DateTime.Now.ToString()}
    };
   }
}
