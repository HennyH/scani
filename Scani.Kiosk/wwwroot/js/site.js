let keySequenceListener = null;

window.registerKeySequenceListener = (dotNetObjRef, methodName, sequenceTimeoutMilliseconds) => {
    if (keySequenceListener != null) {
        throw Error("There is already a key sequence listener.");
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