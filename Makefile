UNITY?=/Applications/Unity/Unity.app/Contents/MacOS/Unity
FILES=$(addprefix Assets/Plugins/iOS/DepthCapture/,DepthCapture.swift DepthCapture.m DepthCapture.cs)

all: export

export:
	$(UNITY) -exportPackage $(FILES) $(abspath depthcapture.unitypackage) \
		-projectPath DepthCapturePlugin \
		-batchmode \
		-nographics \
		-logfile - \
		-quit
