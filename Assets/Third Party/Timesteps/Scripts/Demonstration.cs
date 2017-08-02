using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
 * An example setup to compare different update types.
 */
public class Demonstration : MonoBehaviour
{
    public Transform cameraPivot;
    public Transform updatePivot;
    public Text fps;
    public Text fixedUpdatesPerSecond;

    public enum UpdateType { Update, FixedUpdate, InterpolatedFixed }
    public UpdateType updateType = UpdateType.InterpolatedFixed;
    public int targetFrameRate = 60;
    public float fixedDeltaTime = 1 / 40.0f;
    public bool simulateSutter = false;

    public float rotateSpeed = 8.0f;

    void FixedUpdate()
    {
        // Enable the interpolated fixed update scheme only when used
        bool useInterpolatedFixed = (updateType == UpdateType.InterpolatedFixed);
        cameraPivot.GetComponent<InterpolatedTransform>().enabled = useInterpolatedFixed;

        // If we are moving the camera in FixedUpdate
        if (updateType != UpdateType.Update) {
            RotateCamera();
        }
    }

    void Update()
    {
        // set GUI
        fps.text = "Updates Per Second: " + targetFrameRate;
        fixedUpdatesPerSecond.text = "FixedUpdates Per Second: " + ((int)((1 / fixedDeltaTime) * 1000) / 1000);
        Time.fixedDeltaTime = fixedDeltaTime;

        // sets the transform of the sphere
        updatePivot.Rotate(0, Time.deltaTime * rotateSpeed, 0, Space.World);

        // Sets the target update rate, and can simulate a highly variable frame rate to simulate loading
        // or high complexity rendering frames.
        QualitySettings.vSyncCount = 0;
        if (simulateSutter) {
            Application.targetFrameRate = Random.Range(5, targetFrameRate);
        } else {
            Application.targetFrameRate = targetFrameRate;
        }

        // If we are moving the camera in Update
        if (updateType == UpdateType.Update) {
            RotateCamera();
        }
    }

    private void RotateCamera()
    {
        // When called from FixedUpdate, Time.deltaTime is the fixedDeltaTime
        cameraPivot.Rotate(0, Time.deltaTime * rotateSpeed, 0, Space.World);
    }

    public void ChangeUpdateMode(int val)
    {
        updateType = (UpdateType)val;
    }

    public void ChangeFrameRate(float frameRate)
    {
        targetFrameRate = (int)frameRate;
    }

    public void ChangeFixedDeltaTime(float fixedUpdatesPerSecond)
    {
        fixedDeltaTime = 1.0f / fixedUpdatesPerSecond;
    }

    public void SetSimulateStutter(bool stutter)
    {
        simulateSutter = stutter;
    }
}
