using System.Collections.Generic;
using UnityEngine;

public class ClassCodeDictionary : MonoBehaviour
{
    private static Dictionary<string, string> classCodeDictionary = new Dictionary<string, string>
    {
        { "CC 103", "Computer Programming" },
        { "CC 107", "Web System and Technologies" },
        { "CC 108", "Technical Computer Concepts" },
        { "CCS 110", "Digital Graphics" },
        { "CS 104", "Discrete Structures 2" },
        { "CS 107", "Embedded Programming" },
        { "CS 115", "Human Computer Interaction" },
        { "CS 105", "Programming Languages" },
        { "CCS 106", "Application Development" },
        { "CS 111", "Architecture and Organization" },
        { "CSE 102", "Graphics and Visual Computing" },
        { "CCS 126", "Software Engineering 2" },
        { "CS 113", "Automata and Language Theory" },
        { "CS 119", "CS Thesis Writing 2" },
        { "CCS 115", "Current Trends In IT and Seminars" },
        { "CSE 105", "Parallel and Distributed Computing" },
        { "CS 416", "Practicum 2" }
    };

    public static string GetSubjectName(string classCode)
    {
        if (classCodeDictionary.ContainsKey(classCode))
        {
            return classCodeDictionary[classCode];
        }
        return ""; 
    }
}
