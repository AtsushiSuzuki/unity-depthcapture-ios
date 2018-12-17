using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

public class DepthCapture : IDisposable
{
    public delegate void DepthCaptureEventHandler(
        IntPtr pVideoData,
        int videoWidth,
        int videoHeight,
        IntPtr pDepthData,
        int depthWidth,
        int depthHeight);

    private delegate void DepthCaptureCallback(
        IntPtr pVideoData,
        IntPtr videoWidth,
        IntPtr videoHeight,
        IntPtr pDepthData,
        IntPtr depthWidth,
        IntPtr depthHeight,
        IntPtr state);

    [DllImport("__Internal")]
    private static extern IntPtr DepthCapture_init(DepthCaptureCallback callback, IntPtr state);

    [DllImport("__Internal")]
    private static extern IntPtr DepthCapture_configure(IntPtr capture, string[] deviceTypes, int deviceTypeSize, int position, string preset);

    [DllImport("__Internal")]
    private static extern void DepthCapture_start(IntPtr capture);

    [DllImport("__Internal")]
    private static extern void DepthCapture_stop(IntPtr capture);

    [DllImport("__Internal")]
    private static extern void DepthCapture_release(IntPtr capture);

    public enum DeviceType
    {
        AVCaptureDeviceTypeBuiltInDualCamera,
        AVCaptureDeviceTypeBuiltInMicrophone,
        AVCaptureDeviceTypeBuiltInTelephotoCamera,
        AVCaptureDeviceTypeBuiltInTrueDepthCamera,
        AVCaptureDeviceTypeBuiltInWideAngleCamera,
    }

    public enum Position
    {
        Unspecified = 0,
        Back = 1,
        Front = 2,
    }

    public enum Preset
    {
        AVCaptureSessionPreset352x288,
        AVCaptureSessionPreset1280x720,
        AVCaptureSessionPreset1920x1080,
        AVCaptureSessionPreset3840x2160,
        AVCaptureSessionPresetHigh,
        AVCaptureSessionPresetiFrame1280x720,
        AVCaptureSessionPresetiFrame960x540,
        AVCaptureSessionPresetInputPriority,
        AVCaptureSessionPresetLow,
        AVCaptureSessionPresetMedium,
        AVCaptureSessionPresetPhoto,
        AVCaptureSessionPreset640x480,
    }


    private IntPtr capture_;
    private IntPtr state_;

    public event DepthCaptureEventHandler DepthCaptured;

    public DepthCapture()
    {
        state_ = (IntPtr)GCHandle.Alloc(this);
        capture_ = DepthCapture_init(OnDepthCaptured, state_);
    }

    public void Configure(DeviceType[] deviceTypes = null, Position position = Position.Unspecified, Preset preset = Preset.AVCaptureSessionPreset640x480)
    {
		deviceTypes = deviceTypes ?? new[] {   DeviceType.AVCaptureDeviceTypeBuiltInTrueDepthCamera,DeviceType.AVCaptureDeviceTypeBuiltInDualCamera,};
        var error = DepthCapture_configure(
            capture_,
            deviceTypes.Select(t => t.ToString()).ToArray(),
            deviceTypes.Length,
            (int)position,
            preset.ToString());

        if ((long)error != 0) {
            throw new Exception(string.Format("DepthCapture.configure: error: {0}", error));
        }
    }

    public void Start()
    {
        DepthCapture_start(capture_);
    }

    public void Stop()
    {
        DepthCapture_stop(capture_);
    }

    public virtual void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        DepthCapture_release(capture_);
        GCHandle.FromIntPtr(state_).Free();
    }

    [MonoPInvokeCallback(typeof(DepthCaptureCallback))]
    private static void OnDepthCaptured(
        IntPtr pVideoData,
        IntPtr videoWidth,
        IntPtr videoHeight,
        IntPtr pDepthData,
        IntPtr depthWidth,
        IntPtr depthHeight,
        IntPtr state)
    {
        ((DepthCapture)GCHandle.FromIntPtr(state).Target).OnDepthCaptured(
            pVideoData,
            (int)videoWidth,
            (int)videoHeight,
            pDepthData,
            (int)depthWidth,
            (int)depthHeight);
    }

    protected virtual void OnDepthCaptured(
        IntPtr pVideoData,
        int videoWidth,
        int videoHeight,
        IntPtr pDepthData,
        int depthWidth,
        int depthHeight)
    {
        var handler = DepthCaptured;
        if (handler != null)
        {
            handler(pVideoData, videoWidth, videoHeight, pDepthData, depthWidth, depthHeight);
        }
    }

    public void AcquireNextFrame(DepthCaptureEventHandler action)
    {
        var ev = new AutoResetEvent(false);
        DepthCaptureEventHandler handler = null;
        handler = (pVideoData, videoWidth, videoHeight, pDepthData, depthWidth, depthHeight) =>
        {
            action(pVideoData, videoWidth, videoHeight, pDepthData, depthWidth, depthHeight);
            DepthCaptured -= handler;
            ev.Set();
        };
        DepthCaptured += handler;
        ev.WaitOne();
    }
}
