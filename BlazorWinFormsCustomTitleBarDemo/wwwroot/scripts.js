window.blazorWinFormsCustomTitleBar = {
    initializeKeyboardShortcuts: (dotNetObject) =>
        document.addEventListener('keydown', e => window.blazorWinFormsCustomTitleBar.handleKeyboardShortcut(dotNetObject, e)),

    handleKeyboardShortcut: (dotNetObject, e) => {
        let mappedEvent = {
            key: e.key,
            code: e.code,
            location: e.location,
            repeat: e.repeat,
            ctrlKey: e.ctrlKey,
            shiftKey: e.shiftKey,
            altKey: e.altKey,
            metaKey: e.metaKey,
            isComposing: e.isComposing
        }; // e must be mapped to pass needed properties. e parsed to JSON contains only one entry: 'isTrusted'

        dotNetObject.invokeMethodAsync('HandleKeyboardShortcut', mappedEvent);
    },

    getBodyBackgroundColor: () =>
        window.getComputedStyle(document.body, null).getPropertyValue('background-color')
}
