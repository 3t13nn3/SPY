using System.Collections.Generic;
using UnityEngine;

public class FunctionParam : MonoBehaviour {
	// Comp�tence impl�ment�
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Comp�tence impl�ment�
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quel sont les fonctions � activ� si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctions � desactiv� si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
}