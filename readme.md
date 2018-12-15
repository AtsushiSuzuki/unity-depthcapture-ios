unity-depthcapture-ios
======================
unity-depthcapture-ios is Unity plugin to obtain depth map from iPhone camera.


## Development environment
- Unity 2018.2.18f1
- Xcode 10.1 or later


## Target environment
- iPhone X or later (True Depth Camera)
- or iPhone 7 or later (Dual Camera)


## Usage
1. Import [unity-swift](https://github.com/miyabi/unity-swift) on your Unity project
   <br>or configure Swift-ObjC bridging by yourself
2. Import [depthcapture.unitypackage](https://github.com/AtsushiSuzuki/unity-depthcapture-ios/releases) on your Unity project
3. Conifgure your Xcode project
  - Set Deployment Target to `11.1` or laters
  - Set `SWIFT_VERSION` to `4.2` or later
  - Add `NSCameraUsageDescription` on `Info.plist`
4. Implement camera permission request, see https://docs.unity3d.com/ScriptReference/Application.RequestUserAuthorization.html
5. Use `DepthCapture` class in your script (see [example](DepthCapturePlugin/Assets/CaptureButton.cs))


## Example
```csharp
// set up capture
var capture = new DepthCapture();
capture.Configure();
capture.Start();

// acquire captured frame
int width = 0, height = 0;
float[] pixels = null;
capture.AcquireNextFrame((pVideoData, videoWidth, videoHeight, pDepthData, depthWidth, depthHeight) =>
{
    width = depthWidth;
    height = depthHeight;
    pixels = new float[width * height];
    Marshal.Copy(pDepthData, pixels, 0, width * height);
});

var texture = new Texture2D(width, height);
for (var y = 0; y < (int)height; y++)
{
    for (var x = 0; x < (int)width; x++)
    {
        var v = pixels[y * width + x];
        Color color;
        if (float.IsNaN(v))
        {
            color = new Color(0f, 1f, 0f);
        }
        else
        {
            color = new Color(v, v, v);
        }
        texture.SetPixel(x, y, color);
    }
}

quad.GetComponent<Renderer>().material.mainTexture = texture;
texture.Apply();

// release capture
capture.Stop();
capture.Dispose();
```


## API
### class `DepthCapture`

`DepthCapture` utilizes an [AVCaptureSession](https://developer.apple.com/documentation/avfoundation/avcapturesession) and related objects.

#### constructor `DepthCapture()`
`DepthCapture()` constructs an instance.

#### method `Configure(DeviecType[], Position, Preset)`
`Configure(DeviecType[], Position, Preset)` sets up capture session.

`Configure(DeviecType[], Position, Preset)` can be called only once for each instance.

`Configure(DeviecType[], Position, Preset)` may throw exception if configuration failed.

- Parameter `DeviceType[] deviceTypes`:
  <br>selection criteria for camera device.
  <br>see https://developer.apple.com/documentation/avfoundation/avcapturedevice/discoverysession.
  <br>defaults to `{ DeviceType.AVCaptureDeviceTypeBuiltInDualCamera, DeviceType.AVCaptureDeviceTypeBuiltInTrueDepthCamera }`.
- Parameter `Position position`:
  <br>selection criteria for camera device.
  <br>see https://developer.apple.com/documentation/avfoundation/avcapturedevice/discoverysession.
  <br>defaults to `Position.Unspecified`.
- Parameter `Preset preset`:
  <br>Video settings preset.
  <br>see https://developer.apple.com/documentation/avfoundation/avcapturesession/1389696-sessionpreset.
  <br>defaults to `Preset.AVCaptureSessionPreset640x480`

#### method `Start()`
`Start()` starts capture session.

`Start()` can be called after `Configure` or `Stop`.

#### method `Stop()`
`Stop()` stops capture session.

`Stop()` can be called after `Start`.

#### method `Dispose()`
`Dispose()` release all related resources.

`Dispose()` can be called after `Stop` or `Configure`.

#### event `DepthCaptured`
`DepthCaptured` is invoked when a frame (video image + depth map) is captured.
This event is invoked on non-main thread.

#### method `AcquireNextFrame(DepthCaptureEventHandler action)`
`AcquireNextFrame(DepthCaptureEventHandler action)` waits for next frame captured, and calls provided callback.
The callback is invoked on non-main thread.

#### delegate `DepthCaptureEventHandler(IntPtr pVideoData, int videoWidth, int videoHeight, IntPtr pDepthData, int depthWidth, int depthHeight)`
`DepthCaptureEventHandler(IntPtr pVideoData, int videoWidth, int videoHeight, IntPtr pDepthData, int depthWidth, int depthHeight)` is used by event `DepthCaptured` or method `AcquireNextFrame(DepthCaptureEventHandler action)`.

- Parameter `IntPtr pVideoData`:
  <br>Pointer to video image buffer. pixel format is 32bit BGRA. this pointer is valid only in callback.
- Parameter `int videoWidth`:
  <br>image width for `pVideoData`.
- Parameter `int videoHeight`:
  <br>image height for `pVideoData`.
- Parameter `int pDepthData`:
  <br>Pointer to depth map buffer. pixel format is 32bit float. this pointer is valid only in callback.
  <br>Each pixel value represents distance in meter, see https://developer.apple.com/documentation/avfoundation/avdepthdata.
- Parameter `int depthWidth`:
  <br>depth width for `pDepthData`.
- Parameter `int depthHeight`:
  <br>depth height for `pDepthData`.


## Build unitypackage
```sh
$ make

# generates `depthcapture.unitypackage`
# requires `make`
```


## Build example
1. Open "DepthCapturePlugin" by Unity
2. In "Files" > "Build Settings", select "iOS" as platform
3. Generate Xcode project by "Build and Run"
4. In Xcode, open project settings for "Unity-iPhone" and select your development team
5. In Xcode, "Product" > "Run"


## LICENSE
ISC
