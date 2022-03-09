using UnityEngine;
using FYFY;

public class CameraSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void setLocateButtons(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setLocateButtons", go);
	}

	public void SetCameraSystem(System.Boolean value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCameraSystem", value);
	}

}
