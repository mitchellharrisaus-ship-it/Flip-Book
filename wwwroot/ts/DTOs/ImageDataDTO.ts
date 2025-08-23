export default class ImageDataDTO {
    canvasWidth: number
    canvasHeight: number
    encodedImage: string
    imageName: string
    frameNumber: number
    fileExtension: string

    constructor(canvasWidth: number, canvasHeight: number, encodedImage: string,
                imageName: string, frameNumber: number, fileExtension?: string) {
        this.canvasWidth = canvasWidth
        this.canvasHeight = canvasHeight
        this.encodedImage = encodedImage

        this.imageName = imageName
        this.frameNumber = frameNumber

        this.fileExtension = fileExtension ?? "jpeg"; // Default if not provided
    }
}
