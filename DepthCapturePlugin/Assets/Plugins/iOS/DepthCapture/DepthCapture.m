#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <UnityFramework/UnityFramework-Swift.h>

typedef void (*DepthCaptureCallback)(const void *pVideoData,
                                     intptr_t videoWidth,
                                     intptr_t videoHeight,
                                     const void *pDepthData,
                                     intptr_t depthWidth,
                                     intptr_t depthHeight,
                                     const void *state);

const void *DepthCapture_init(DepthCaptureCallback callback, const void *state) {
    return CFBridgingRetain([[DepthCapture alloc] initWithCallback:callback state:state]);
}

const intptr_t DepthCapture_configure(const void *capture, const char **deviceTypes, const int deviceTypeSize, const int position, const char *preset) {
    NSMutableArray<NSString *> *array = [NSMutableArray<NSString *> arrayWithCapacity:deviceTypeSize];
    for (int i = 0; i < deviceTypeSize; i++) {
        array[i] = [NSString stringWithCString:deviceTypes[i] encoding:NSASCIIStringEncoding];
    }
    return [(__bridge DepthCapture*)capture configureWithDeviceTypes:array
                                                            position:position
                                                              preset:[NSString stringWithCString:preset encoding:NSASCIIStringEncoding]];
}

void DepthCapture_start(const void *capture) {
    [(__bridge DepthCapture*)capture start];
}

void DepthCapture_stop(const void *capture) {
    [(__bridge DepthCapture*)capture stop];
}

void DepthCapture_release(const void *capture) {
    CFBridgingRelease(capture);
}
