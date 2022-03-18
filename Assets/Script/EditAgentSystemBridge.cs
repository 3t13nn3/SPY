using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditAgentSystemBridge : MonoBehaviour
{
	// Agent associ� au script
	private GameObject agent;

	void Start()
	{

	}

	public void agentSelect(BaseEventData agent)
    {
		Debug.Log("agent selectionn�");
		EditAgentSystem.instance.agentSelect(agent);
	}

	// Methode appell�e pour changer le nom de l'agent
	public void setAgentName(string newName)
	{
		EditAgentSystem.instance.setAgentName(newName);
	}
}
