import BrushType from "../Types/BrushType"
import Colour from "../Types/Colour"
import { Vertex } from "../Types/Vertex"

export default class DrawActionDTO {
    vertices: Vertex[]
    brush: BrushType
    brushColour: Colour

    constructor() {
        this.vertices = []
        this.brush = BrushType.Pen
        this.brushColour = { r: 255, g: 255, b: 255, a: 0 }
    }
}
