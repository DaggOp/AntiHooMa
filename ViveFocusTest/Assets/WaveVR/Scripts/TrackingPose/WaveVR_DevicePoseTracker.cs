﻿// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using wvr;
using WaveVR_Log;

/// <summary>
/// This class is mainly for Device Tracking.
/// Tracking object communicates with HMD device or controller device in order to:
/// update new position and rotation for rendering
/// </summary>
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_DevicePoseTracker))]
public class WaveVR_DevicePoseTrackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaveVR_DevicePoseTracker myScript = target as WaveVR_DevicePoseTracker;

        myScript.type = (WVR_DeviceType)EditorGUILayout.EnumPopup ("Type", myScript.type);
        myScript.inversePosition = EditorGUILayout.Toggle ("Inverse Position", myScript.inversePosition);
        myScript.trackPosition = EditorGUILayout.Toggle ("Track Position", myScript.trackPosition);
        if (true == myScript.trackPosition)
        {
            if (myScript.type == WVR_DeviceType.WVR_DeviceType_HMD)
            {
                myScript.EnableNeckModel = (bool)EditorGUILayout.Toggle ("    Enable Neck Model", myScript.EnableNeckModel);
            }
        }

        myScript.inverseRotation = EditorGUILayout.Toggle ("Inverse Position", myScript.inverseRotation);
        myScript.trackRotation = EditorGUILayout.Toggle ("Track Rotation", myScript.trackRotation);
        myScript.timing = (WVR_TrackTiming)EditorGUILayout.EnumPopup ("Track Timing", myScript.timing);

        if (GUI.changed)
            EditorUtility.SetDirty ((WaveVR_DevicePoseTracker)target);
    }
}
#endif

public sealed class WaveVR_DevicePoseTracker : MonoBehaviour
{
    private static string LOG_TAG = "WaveVR_DevicePoseTracker";
    /// <summary>
    /// The type of this controller device, it should be unique.
    /// </summary>
    public WVR_DeviceType type;
    public bool inversePosition = false;
    public bool trackPosition = true;
    [Tooltip("Effective only when Track Position is true.")]
    public bool EnableNeckModel = true;
    public bool inverseRotation = false;
    public bool trackRotation = true;

    public WVR_TrackTiming timing = WVR_TrackTiming.WhenNewPoses;

    #if UNITY_EDITOR
    private WVR_DevicePosePair_t wvr_pose = new WVR_DevicePosePair_t ();
    private WaveVR_Utils.RigidTransform rigid_pose = WaveVR_Utils.RigidTransform.identity;
    #endif

    void Update()
    {
        if (timing == WVR_TrackTiming.WhenNewPoses)
            return;
        if (WaveVR.Instance == null)
            return;

        WaveVR.Device device = WaveVR.Instance.getDeviceByType (this.type);
        if (device != null && device.connected)
        {
            updatePose (device.pose, device.rigidTransform);
        }
    }


    /// if device connected, get new pose, then update new position and rotation of transform
    private void OnNewPoses(params object[] args)
    {
#if UNITY_EDITOR
        if (Application.isEditor && !WaveVR.Instance.isSimulatorOn)
        {
            WVR_DeviceType _type = WaveVR_Controller.Input(this.type).DeviceType;
            var system = WaveVR_PoseSimulator.Instance;
            system.GetTransform (_type, ref wvr_pose, ref rigid_pose);
            updatePose(wvr_pose, rigid_pose);
        } else
#endif
        {
            WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.type);
            if (_device.connected)
            {
                updatePose (_device.pose, _device.rigidTransform);
            }
        }
    }

    void updatePose(WVR_DevicePosePair_t pose, WaveVR_Utils.RigidTransform rtPose)
    {
        if (trackPosition)
        {
            if (inversePosition)
                transform.localPosition = -rtPose.pos;
            else
            {
                transform.localPosition = rtPose.pos;
            }
        }
        if (trackRotation)
        {
            if (inverseRotation)
                transform.localRotation = Quaternion.Inverse(rtPose.rot);
            else
                transform.localRotation = rtPose.rot;
        }
    }

    void OnEnable()
    {
        if (this.timing == WVR_TrackTiming.WhenNewPoses)
            WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.NEW_POSES, OnNewPoses);

        #if UNITY_EDITOR
        if (Application.isEditor)
            return;
        else
        #endif
        {
            Log.i (LOG_TAG, "OnEnable() WVR_SetNeckModelEnabled to " + EnableNeckModel);
            Interop.WVR_SetNeckModelEnabled (EnableNeckModel);
        }
    }

    void OnDisable()
    {
        if (this.timing == WVR_TrackTiming.WhenNewPoses)
            WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.NEW_POSES, OnNewPoses);
    }
}

