using System.Collections.Generic;
using UnityEngine;

public class FunctionalityParam : MonoBehaviour {
	// Fonctionalit� impl�ment�
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Fonctionalit� bas� sur le level design
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quel sont les fonctionalit�s � activ� si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctionalit�s � desactiv� si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
}