using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Flipbook_App.Pages;

public class CanvasModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public CanvasModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void OnGet()
    {
    }

    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> OnPostSaveAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON and extract imageData
            var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("imageData", out var imageDataElement))
                return BadRequest("Missing imageData property.");

            var base64Data = imageDataElement.GetString();
            if (string.IsNullOrWhiteSpace(base64Data))
                return BadRequest("imageData is empty.");

            // Remove the data URL prefix
            var base64 = base64Data.Substring(base64Data.IndexOf(",") + 1);
            var bytes = Convert.FromBase64String(base64);

            // Use the web root (wwwroot) as the base path
            var folderPath = Path.Combine(_env.WebRootPath, "Animations", "CanvasUploads");
            Directory.CreateDirectory(folderPath);

            // Save the file (you can use a unique name if needed)
            var filePath = Path.Combine(folderPath, "saved-drawing.png");
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest($"Exception: {ex.Message}");
        }
    }
}
