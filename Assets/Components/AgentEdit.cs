using UnityEngine;

public class AgentEdit : MonoBehaviour
{
	public enum EditMode { 
		Locked, // Le nom est d�fini par le syst�me
		Editable, // On autorise le changement de nom par l'utilisateur
		Synch // Si on change le nom du script container, cela change aussi le nom de l'agent associ�
	};
	// Pour l'�dition du nom
	public string agentName = "N�1"; //Nom par defaut
	public EditMode editState = EditMode.Synch;
}