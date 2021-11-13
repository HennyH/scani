let keySequenceListener = null;

window.addEventListener("load", () => {
    const height = window.innerHeight * 0.01;
    document.body.style.setProperty("--vh", `${height}px`);
})

let $codeReader = new Promise((resolve, reject) => {
    window.addEventListener('load', function () {
        const zx = new ZXing.BrowserMultiFormatReader();
        zx.timeBetweenScansMillis = 50;
        resolve(zx);
    })
});

window.clearVideoStreams = (selector) => {
    const videoElements = document.querySelectorAll(selector);
    videoElements.forEach(videoElement => {
        if (videoElement === null || videoElement === undefined || videoElement.srcObject === null || videoElement.srcObject === undefined) return;
        const tracks = videoElement.srcObject.getTracks();

        tracks.forEach(t => t.stop());

        videoElement.srcObject = null;
    });
}

window.ZXingListVideoInputDevices = async () => {
    const zx = await $codeReader;
    try {
        await navigator.mediaDevices.getUserMedia({ audio: false, video: true })
    } finally {
        return await zx.listVideoInputDevices();
    }
}

window.ZXingResetCodeReader = async (videoElementId) => {
    const zx = await $codeReader;
    clearVideoStreams(videoElementId);
    zx.enable = false;
    zx.reset();
}

window.ZXingRegisterOnDecodeListener = async (dotNetObjRef, onResultMethodName, onErrorMethodName, deviceId, videoElementId, sameCodeTimeoutMilliseconds) => {

    let codeToLastScannedTime = {};

    const zx = await $codeReader;
    zx.enable = true;
    zx.decodeFromVideoDevice(deviceId, videoElementId, async (result, err) => {
        if (result) {
            const now = new Date();
            const code = result.getText();
            if (codeToLastScannedTime[code] === undefined || (now - codeToLastScannedTime[code]) >= sameCodeTimeoutMilliseconds) {
                codeToLastScannedTime[code] = now;
                await dotNetObjRef.invokeMethodAsync(onResultMethodName, result.getText());
            }
        } else if (err && !(err instanceof ZXing.NotFoundException)) {
            await dotNetObjRef.invokeMethodAsync(onErrorMethodName, err.toString());
            console.error(err);
        }
    });
}

const videoElementIdToAlreadyPlaying = {};

window.streamMediaDeviceIntoVideoElement = async (elementId, deviceId) => {
    try {
        const videoElement = document.getElementById(elementId);
        if (deviceId !== null) {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    deviceId: { exact: deviceId }
                }
            });
            console.log("showing stream from", deviceId, stream, "on element", elementId);
            videoElement.srcObject = null;
            setTimeout(() => {
                videoElement.srcObject = stream;
                setTimeout(() => {
                    try {
                        videoElement.play();
                    } catch (error) {

                    }
                });
            });
        } else {
            clearVideoStreams(elementId);
        }
    } catch (err) {
        console.error(err);
    }
}

window.registerKeySequenceListener = (dotNetObjRef, methodName, sequenceTimeoutMilliseconds) => {
    if (keySequenceListener != null) {
        deregisterKeySequenceListener(keySequenceListener);
    }

    let lastAlteredTime = null;
    let keySequence = [];

    keySequenceListener = keyupEvent => {
        var now = new Date();
        if (lastAlteredTime !== null && (now - lastAlteredTime) >= sequenceTimeoutMilliseconds) {
            console.log(keySequence, now, lastAlteredTime, now - lastAlteredTime, sequenceTimeoutMilliseconds, "timeout elapsed");
            keySequence = [];
        }

        if (keyupEvent.target == document.body) {
            if (keyupEvent.key == "Enter" && keySequence.length > 0) {
                const keys = [...keySequence];
                keySequence = [];
                lastAlteredTime = null;
                return dotNetObjRef.invokeMethodAsync(methodName, keys);

            } else if (!keyupEvent.repeat) {
                keySequence.push(keyupEvent.key);
                lastAlteredTime = now;
            }
        }
    };

    window.addEventListener("keyup", keySequenceListener);
}

window.deregisterKeySequenceListener = () => {
    if (keySequenceListener != null) {
        window.removeEventListener("keyup", keySequenceListener);
    }
    keySequenceListener = null;
}

const elementIdToModalInstance = {}

window.InitializeModal = (elementId, dotNetObjRef, onDismissedMethodName) => {
    const element = document.getElementById(elementId);
    const modal = new bootstrap.Modal(element);
    element.addEventListener("hide.bs.modal", async () => {
        await dotNetObjRef.invokeMethodAsync(onDismissedMethodName);
    });
    elementIdToModalInstance[elementId] = modal;
}

window.ShowModal = (elementId) => {
    try {
        elementIdToModalInstance[elementId].show();
    } catch (err) {
        console.warn(err);
    }
}

window.HideModal = (elementId) => {
    try {
        elementIdToModalInstance[elementId].hide();
    } catch (err) {
        console.warn(err);
    }
}

window.DisposeModal = (elementId) => {
    try {
        elementIdToModalInstance[elementId].dispose();
    } catch (err) {
        console.warn(err);
    }
}