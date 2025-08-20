/// <reference types="p5/global" />

let canvas: HTMLCanvasElement | null = null

let resolveCanvas: (value: HTMLCanvasElement) => void
let canvasLoaded: Promise<HTMLCanvasElement> = new Promise(resolve  => {
    resolveCanvas = resolve
})

export async function getCanvas(): Promise<HTMLCanvasElement> {
    return await canvasLoaded
}

export function getPixels(): Uint8ClampedArray | null {
    if (!canvas) return null

    return canvas.getContext('2d')?.getImageData(0, 0, canvas.width, canvas.height).data || null
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