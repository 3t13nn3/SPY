using UnityEngine;
using FYFY;

public class TimerSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void pauseTimer()
	{
		MainLoop.callAppropriateSystemMethod (system, "pauseTimer", null);
	}

	public void resumeTimer()
	{
		MainLoop.callAppropriateSystemMethod (system, "resumeTimer", null);
	}

}
