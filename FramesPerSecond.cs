using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; //textmesh pro

//An accurate fps counter that uses a frame buffer to get average framerate
public class FramesPerSecond : MonoBehaviour {

    public float updateFrequency; //update UI every x seconds
	public TMP_Text fpsText;
	public int fpsBufferSize = 60; //num of values to buffer
	public int AverageFPS;
	int[] fpsBuffer;
	int fpsBufferIndex;

	// Use this for initialization
	void Start () 
	{
		StartCoroutine(UpdateFPS());
	}

	IEnumerator UpdateFPS()
	{
		WaitForSeconds wait = new WaitForSeconds(updateFrequency);

        while(true)
        {
            yield return wait;
			CalculateFPS();
			if(fpsText)
			{
				fpsText.text = AverageFPS.ToString();
			}
        }
	}

	void Update () {
		if (fpsBuffer == null || fpsBuffer.Length != fpsBufferSize) {
			InitializeBuffer();
		}
		UpdateBuffer();
	}

	//make our array
	void InitializeBuffer () {
		if (fpsBufferSize <= 0) {
			fpsBufferSize = 1;
		}
		fpsBuffer = new int[fpsBufferSize];
		fpsBufferIndex = 0;
	}

	//put new value in the buffer
	void UpdateBuffer () {
		fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
		if (fpsBufferIndex >= fpsBufferSize) {
			fpsBufferIndex = 0;
		}
	}

	//iterate through the buffer and get an average framerate
	public void CalculateFPS () 
	{
		int sum = 0;
		for (int i = 0; i < fpsBufferSize; i++) {
			sum += fpsBuffer[i];
		}
		AverageFPS = sum / fpsBufferSize;
	}
}
