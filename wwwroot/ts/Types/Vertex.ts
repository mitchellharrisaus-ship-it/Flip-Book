
export interface Vertex {
    x: number
    y: number
}

export class VertexObj implements Vertex {
    x: number
    y: number

    constructor(x: number, y: number) {
        this.x = x
        this.y = y
    }
}
