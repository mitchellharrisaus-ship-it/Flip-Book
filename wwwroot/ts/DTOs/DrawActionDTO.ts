import BrushType from "../Types/BrushType"
import Colour from "../Types/Colour"
import { Vertex } from "../Types/Vertex"

export default class DrawActionDTO {
    vertices: Vertex[]

    brush: BrushType
    brushColour: Colour
    brushSize: number

    actionFrame: number // The frame number this action was created in
    //actionTimestamp: number

    constructor() {
        this.vertices = []

        this.brush = BrushType.Pen
        this.brushColour = { r: 0, g: 0, b: 0, a: 120 }
        this.brushSize = 3

        this.actionFrame = 0
        //this.actionTimestamp = Date.now()
    }
}
