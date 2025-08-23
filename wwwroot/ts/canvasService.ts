/// <reference types="p5/global" />

import DrawActionDTO from "./DTOs/DrawActionDTO.js"
import ImageDataDTO from "./DTOs/ImageDataDTO.js"

let canvas: HTMLCanvasElement | null = null

let resolveCanvas: (value: HTMLCanvasElement) => void
let canvasLoaded: Promise<HTMLCanvasElement> = new Promise(resolve  => {
    resolveCanvas = resolve
})

export async function getCanvas(): Promise<HTMLCanvasElement> {
    return await canvasLoaded
}

export async function getCanvasRect(): Promise<DOMRect> {
    return await canvasLoaded
        .then(canvas => canvas.getBoundingClientRect())
}

export async function writeCanvasToFile(): Promise<void> {
    if (!canvas) {
        await canvasLoaded
    }

    // might need to change file type here
    const encodedImage = canvas?.toDataURL('image/png', 1.0)
    if (!encodedImage) {
        console.error("Failed to encode canvas to image")
        return
    }

    const canvasRect = await getCanvasRect()
    const message = new ImageDataDTO(canvasRect.width, canvasRect.height, encodedImage, "my first animation", 0)

    await fetch("api/canvas/write-to-file", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(message)
    })
}

export async function writeActionToFile(action: DrawActionDTO): Promise<void> {
    if (!canvas) {
        await canvasLoaded
    }

    await fetch("api/canvas/write-action-to-file", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(action)
    })
}

export function clearCanvas() {
    if (!canvas) return
    const ctx = canvas.getContext('2d')
    if (ctx) {
        ctx.clearRect(0, 0, canvas.width, canvas.height)
    }
}

// --- initialise canvas ---
//auto-ran setup function when the script is loaded due to p5.js
(window as any).setup = () => {
    let p5Canvas = createCanvas(800, 600)
    p5Canvas.class("canvas")

    canvas = p5Canvas.elt as HTMLCanvasElement

    resolveCanvas(canvas)
}