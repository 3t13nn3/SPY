using UnityEngine;

public class BasicVariable : BaseElement {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public enum VariableType { PositionRobot };
    public VariableType variableType;
}