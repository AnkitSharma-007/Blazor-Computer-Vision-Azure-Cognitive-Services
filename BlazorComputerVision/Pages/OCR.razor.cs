using BlazorComputerVision.Data;
using BlazorComputerVision.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlazorComputerVision.Pages
{
    public class OCRModel : ComponentBase
    {
        [Inject]
        protected ComputerVisionService computerVisionService { get; set; }

        protected string DetectedTextLanguage;
        protected string imagePreview;
        protected bool loading = false;
        byte[] imageFileBytes;

        const string DefaultStatus = "Maximum size allowed for the image is 4 MB";
        protected string status = DefaultStatus;

        protected OcrResultDTO Result = new OcrResultDTO();
        private AvailableLanguage availableLanguages;
        private Dictionary<string, LanguageDetails> LanguageList = new Dictionary<string, LanguageDetails>();
        const int MaxFileSize = 4 * 1024 * 1024; // 4MB

        protected override async Task OnInitializedAsync()
        {
            availableLanguages = await computerVisionService.GetAvailableLanguages();
            LanguageList = availableLanguages.Translation;
        }

        protected async Task ViewImage(InputFileChangeEventArgs e)
        {
            if (e.File.Size > MaxFileSize)
            {
                status = $"The file size is {e.File.Size} bytes, this is more than the allowed limit of {MaxFileSize} bytes.";
                return;
            }
            else if (!e.File.ContentType.Contains("image"))
            {
                status = "Please uplaod a valid image file";
                return;
            }
            else
            {
                using var reader = new StreamReader(e.File.OpenReadStream(MaxFileSize));
                var format = "image/jpeg";
                var imageFile = await e.File.RequestImageFileAsync(format, 640, 480);

                using var fileStream = imageFile.OpenReadStream(MaxFileSize);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                imageFileBytes = memoryStream.ToArray();
                imagePreview = string.Concat("data:image/png;base64,", Convert.ToBase64String(memoryStream.ToArray()));

                status = DefaultStatus;
            }
        }

        protected private async Task GetText()
        {
            if (imageFileBytes != null)
            {
                loading = true;
                Result = await computerVisionService.GetTextFromImage(imageFileBytes);
                if (LanguageList.ContainsKey(Result.Language))
                {
                    DetectedTextLanguage = LanguageList[Result.Language].Name;
                }
                else
                {
                    DetectedTextLanguage = "Unknown";
                }
                loading = false;
            }
        }
    }
}
