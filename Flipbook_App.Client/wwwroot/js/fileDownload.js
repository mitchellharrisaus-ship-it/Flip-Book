// This is a hacky little thing to get Blazor to download a file.
// When using a http client, the client.PostAsync() uses an AJAX query, which cannot trigger a file download.
window.saveAsFile = (fileName, bytesBase64) => {
    const link = document.createElement('a');
    link.download = fileName;
    const blob = new Blob([Uint8Array.from(atob(bytesBase64), c => c.charCodeAt(0))]);
    link.href = URL.createObjectURL(blob);
    link.click();
};