using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMasterEvent", menuName = "Events/Master Event")]
public class MasterEvent : ScriptableObject
{
    public List<GameObject> Survivors;
    public List<GameObject> Vehicles;

    public List<Event> events = new List<Event>();
}