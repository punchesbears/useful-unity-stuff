using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class LowLatencyMicrophoneInput : MonoBehaviour
{
    public string micDeviceName = "Yeti Stereo Microphone"; //the name of your device in your system's audio device manager
    public float currentOutputVolume; //the current output volume of the specified AudioSource, using the RMS method.
	AudioSource source;
    bool foundMicDevice = false;
    bool sourcePlayed = false;
	// int numSyncIterations = 30;
	public int numSyncIterations = 60;
    public int numSamplesLatency = 512;

    void OnEnable()
    {
        if(Microphone.devices.Contains(micDeviceName))
        {
            foundMicDevice = true; //valid mic device

            //Attempts to adjust settings for less latency
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.dspBufferSize = 256;
            AudioSettings.Reset(config);

            source = GetComponent<AudioSource>();
            source.priority = 0;
            source.Stop();
            source.loop = true;
            source.clip = Microphone.Start(micDeviceName, true, 1, AudioSettings.outputSampleRate); //we're gonna loop a 1 sec clip
        }
        else
        {
            Debug.Log("hey handsome, doublecheck that mic name"); //not a valid mic device
        }
    }

	void Update()
	{
        if (foundMicDevice)
        {
            currentOutputVolume = GetRMS(); //get the current output volume

            if (numSyncIterations > 0)
                numSyncIterations--;

            if (!sourcePlayed && (numSyncIterations == 0) && (Microphone.GetPosition(micDeviceName) > numSamplesLatency))
            {
                source.Play();
                source.timeSamples = Microphone.GetPosition(micDeviceName) - numSamplesLatency;
                sourcePlayed = true;
            }

            if ((source.timeSamples > Microphone.GetPosition(micDeviceName)) && (Microphone.GetPosition(micDeviceName) > numSamplesLatency * 4))
            {
                Debug.Log("INCREASING LATENCY: source.timeSamples " + numSamplesLatency + ", Microphone.GetPosition " + Microphone.GetPosition(micDeviceName) + ", numSamplesLatency " + numSamplesLatency*2);
                numSamplesLatency = numSamplesLatency * 2;
                source.timeSamples = Microphone.GetPosition(micDeviceName) - numSamplesLatency;                
            }
        }
	}

    void OnDisable()
    {
        if (foundMicDevice)
        {
            source.Stop();
            Microphone.End(micDeviceName);
        }

        foundMicDevice = false;
        sourcePlayed = false;
    }


    /// <summary>
    /// Returns the current output volume of the specified AudioSource, using the RMS method.
    /// </summary>
    /// <param name="source">The AudioSource to reference.</param>
    /// <param name="sampleSize">The number of samples to take, as a power of two. Higher values mean more precise volume.</param>
    /// <param name="channelUsed">The audio channel to take data from.</param>
    public float GetRMS(int sampleSize = 1024, int channelUsed = 0)
    {
        sampleSize = Mathf.ClosestPowerOfTwo(sampleSize);
        float[] outputSamples = new float[sampleSize];
        source.GetOutputData(outputSamples, channelUsed);

        float rms = 0;
        foreach (float f in outputSamples)
        {
            rms += f * f; //sum of squares
        }
        return Mathf.Sqrt(rms / (outputSamples.Length)); //mean and root
    }

}
