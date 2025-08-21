export default class ImageDataDTO {
    canvasWidth: number
    canvasHeight: number
    encodedImage: string
    imageName: string
	fileExtension: string

    constructor(canvasWidth: number, canvasHeight: number, encodedImage: string,
                imageName: string, fileExtension?: string) {
        this.canvasWidth = canvasWidth
        this.canvasHeight = canvasHeight
        this.encodedImage = encodedImage

        this.imageName = imageName
        this.fileExtension = fileExtension ?? "png"; // Default to png if not provided
    }
}
