window.getCanvasRect = () => {
    const canvas = document.getElementById("canvasArea")
    if (!canvas) {
        console.error("Couldn't find canvas element, no element has an id of 'canvasArea'")
        return;
    }

    const rect = canvas.getBoundingClientRect();
    return { left: rect.left, top: rect.top, width: rect.width, height: rect.height };
}