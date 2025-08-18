"use strict"

const clearButton = document.getElementById("clearButton")

let clearEvent = () => { }
let isDrawing = false

class Vertex {
    constructor(x, y) {
        this.x = x
        this.y = y
    }
}

let shapes = [] // :[Vertex[]]
let currentShape = [] // :Vertex[]

const sketch = (p) => {
    p.setup = () => {
        const canvas = p.createCanvas(800, 500)
        canvas.class("canvas")
    }

    p.draw = () => {
        p.noFill()
        if (isDrawing) {
            createStroke()
            createVertex(p.mouseX, p.mouseY)
        }
    }

    p.mousePressed = () => {
        isDrawing = true
        p.beginShape()
        // initial vertex is used as reference so we need to create two
        // otherwise the initial vertex will not show up in the end
        createVertex(p.pmouseX, p.pmouseY)
        createVertex(p.pmouseX, p.pmouseY)
    }

    p.mouseReleased = () => {
        isDrawing = false
        createVertex(p.mouseX, p.mouseY)
        p.endShape()

        p.clear()

        completeCurrentShape()

        shapes.forEach(shape => drawShape(shape))
    }

    function createVertex(x, y) {
        p.curveVertex(p.mouseX, p.mouseY)
        currentShape.push(new Vertex(x, y))
    }

    function createStroke() {
        p.stroke(0)
        p.strokeWeight(3)

        p.line(p.pmouseX, p.pmouseY,
               p.mouseX, p.mouseY)
    }

    function drawShape(shape /*: Vertex[]*/) {
        p.beginShape()

        shape.forEach(vertex => {
            p.curveVertex(vertex.x, vertex.y)
        })

        p.endShape()
    }

    function completeCurrentShape() {
        let shallowClone = currentShape.slice()
        currentShape = []
        
        shapes.push(shallowClone)
    }

    clearEvent = () => {
        shapes = []
        currentShape = []
        p.clear()
        isDrawing = false
    }
}

clearButton.addEventListener("click", () => {
    clearEvent()
})

new p5(sketch)