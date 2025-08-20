/// <reference types="p5/global" />
import { VertexObj } from "./Types/Vertex.js"
import { getCanvas, getPixels } from "./canvasService.js"

let isDrawing: boolean = false
let canvas: HTMLCanvasElement = await getCanvas()
let canvasSize: DOMRect = canvas.getBoundingClientRect()

console.log("Canvas size:", canvasSize)

interface MousePosition {
    x: number
    y: number

    previousX: number
    previousY: number
}

let shapes: VertexObj[][] = []
let currentShape: VertexObj[] = []

// --- pointer events ---
canvas.addEventListener('pointermove', (event: PointerEvent) => {
    const mousePosition = GetMousePosition(event)

    if (isDrawing) {
        createVertex(mousePosition.x, mousePosition.y)
        createStroke(mousePosition.previousX, mousePosition.previousY, mousePosition.x, mousePosition.y)
    }
})

canvas.addEventListener('pointerdown', (event: PointerEvent) => {
    isDrawing = true
    beginShape()
})

canvas.addEventListener('pointerup', (event: PointerEvent) => {
    isDrawing = false
    //clear()
    //endShape()
    //const canvasBuffer = loadPixels()
    //fetch()
})

// --- UI ---
const clearButton = document.getElementById("clearButton")

clearButton?.addEventListener("click", () => {
    shapes = []
    currentShape = []
    clear()
    isDrawing = false
})

// --- Logic ---
function createVertex(x: number, y: number) {
    /* Avoid curverVertex for now as it would need to be implemented
       in the BE, although it should be added because it looks very nice.
       Maybe only for like the PEN tool */
    curveVertex(x, y)
    //vertex(x,y)
    currentShape.push(new VertexObj(x, y))
}

function createStroke(x1: any, y1: number, x2: number, y2: number) {
    stroke(0)
    strokeWeight(3)

    line(x1, y1, x2, y2)
}

let prevX: number | undefined, prevY: number | undefined
function GetMousePosition(event: PointerEvent): MousePosition {
    if (prevX === undefined) prevX = event.x
    if (prevY === undefined) prevY = event.y

    const mousePosition: MousePosition = {
        x: event.x - canvasSize.left,
        y: event.y - canvasSize.top,
        previousX: prevX,
        previousY: prevY,
    }

    prevX = mousePosition.x
    prevY = mousePosition.y

    return mousePosition
}