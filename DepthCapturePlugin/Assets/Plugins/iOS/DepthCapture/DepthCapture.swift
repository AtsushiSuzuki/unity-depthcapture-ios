import Foundation
import AVFoundation

class DepthCaptureBase: NSObject, AVCaptureDataOutputSynchronizerDelegate {
    enum Error: Int, Swift.Error {
        case noCameraDeviceFound = 1
        case noCaptureConnection
        case noConfigurationFound
        case noFormatFound
    }

    let queue = DispatchQueue(label: "com.gitlab.suzuki_ds.unity-depthcapture.DepthCapture.queue")
    let session = AVCaptureSession()
    let videoOutput = AVCaptureVideoDataOutput()
    let depthOutput = AVCaptureDepthDataOutput()
    var outputSynchronizer: AVCaptureDataOutputSynchronizer?

    func configure(deviceTypes: [AVCaptureDevice.DeviceType] = [.builtInDualCamera, .builtInTrueDepthCamera],
                   position: AVCaptureDevice.Position = .unspecified,
                   preset: AVCaptureSession.Preset = .vga640x480) throws {
        try queue.sync {
            let discovery = AVCaptureDevice.DiscoverySession(deviceTypes: deviceTypes,
                                                             mediaType: .video,
                                                             position: position)
            guard let device = discovery.devices.first else {
                throw Error.noCameraDeviceFound
            }

            session.beginConfiguration()
            session.sessionPreset = preset
            session.addInput(try AVCaptureDeviceInput(device: device))

            session.addOutput(videoOutput)
            videoOutput.videoSettings = [kCVPixelBufferPixelFormatTypeKey as String: Int(kCVPixelFormatType_32BGRA)]

            session.addOutput(depthOutput)
            depthOutput.isFilteringEnabled = false
            if let connection = depthOutput.connection(with: .depthData) {
                connection.isEnabled = true
            } else {
                throw Error.noCaptureConnection
            }

            let format = device.activeFormat.supportedDepthDataFormats.filter {
                return CMFormatDescriptionGetMediaSubType($0.formatDescription) == kCVPixelFormatType_DepthFloat32
                }.max { first, second  in
                    return CMVideoFormatDescriptionGetDimensions(first.formatDescription).width
                        < CMVideoFormatDescriptionGetDimensions(second.formatDescription).width
            }
            if format == nil {
                throw Error.noFormatFound
            }

            try device.lockForConfiguration()
            device.activeDepthDataFormat = format
            device.unlockForConfiguration()

            outputSynchronizer = AVCaptureDataOutputSynchronizer(dataOutputs: [videoOutput, depthOutput])
            outputSynchronizer!.setDelegate(self, queue: queue)

            session.commitConfiguration()
        }
    }

    func start() {
        queue.sync {
            session.startRunning()
        }
    }

    func stop() {
        queue.sync {
            session.stopRunning()
        }
    }

    func depthCapture(videoData: CMSampleBuffer, depthData: AVDepthData, timestamp: CMTime) {
    }

    func dataOutputSynchronizer(_ synchronizer: AVCaptureDataOutputSynchronizer, didOutput synchronizedDataCollection: AVCaptureSynchronizedDataCollection) {
        guard let syncedVideoData = synchronizedDataCollection.synchronizedData(for: videoOutput) as? AVCaptureSynchronizedSampleBufferData,
              !syncedVideoData.sampleBufferWasDropped,
              let syncedDepthData = synchronizedDataCollection.synchronizedData(for: depthOutput) as? AVCaptureSynchronizedDepthData,
              !syncedDepthData.depthDataWasDropped else {
            return
        }
        depthCapture(videoData: syncedVideoData.sampleBuffer, depthData: syncedDepthData.depthData, timestamp: syncedVideoData.timestamp)
    }
}

typealias DepthCaptureCallback = @convention(c) (UnsafeRawPointer, Int, Int, UnsafeRawPointer, Int, Int, UnsafeRawPointer) -> Void

class DepthCapture: DepthCaptureBase {
    let callback: DepthCaptureCallback
    let state: UnsafeRawPointer

    @objc init(callback: @escaping DepthCaptureCallback, state: UnsafeRawPointer) {
        self.callback = callback
        self.state = state
    }

    @objc func configure(deviceTypes: [String],
                         position: Int,
                         preset: String) -> Int {
        do {
            try super.configure(deviceTypes: deviceTypes.map { AVCaptureDevice.DeviceType(rawValue: $0) },
                                position: AVCaptureDevice.Position(rawValue: position)!,
                                preset: AVCaptureSession.Preset(rawValue: preset))
        } catch {
            return (error as NSError).code
        }
        return 0
    }

    @objc override func start() {
        super.start()
    }

    @objc override func stop() {
        super.stop()
    }

    override func depthCapture(videoData: CMSampleBuffer, depthData: AVDepthData, timestamp: CMTime) {
        let videoBuffer = CMSampleBufferGetImageBuffer(videoData)!
        let depthBuffer = depthData.depthDataMap
        CVPixelBufferLockBaseAddress(videoBuffer, [.readOnly])
        CVPixelBufferLockBaseAddress(depthBuffer, [.readOnly])
        callback(CVPixelBufferGetBaseAddress(videoBuffer)!,
                 CVPixelBufferGetWidth(videoBuffer),
                 CVPixelBufferGetHeight(videoBuffer),
                 CVPixelBufferGetBaseAddress(depthBuffer)!,
                 CVPixelBufferGetWidth(depthBuffer),
                 CVPixelBufferGetHeight(depthBuffer),
                 state)
        CVPixelBufferUnlockBaseAddress(videoBuffer, [.readOnly])
        CVPixelBufferUnlockBaseAddress(depthBuffer, [.readOnly])
    }
}
