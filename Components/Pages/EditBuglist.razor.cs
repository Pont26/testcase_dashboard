using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms; // Required for IBrowserFile
using Radzen;
using Radzen.Blazor;
using TestCaseDashboard.Models.mydatabase;

namespace TestCaseDashboard.Components.Pages
{
    public partial class EditBuglist
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }
        
        [Inject]
        public mydatabaseService mydatabaseService { get; set; }

        [Parameter]
        public Guid Id { get; set; }

        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.Buglist buglist;
        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Testcase> testcasesForTestcaseid;

        private string imagePreview;
        private IBrowserFile uploadedFile;

        protected bool hasChanges = false;
        protected bool canEdit = true;

        protected override async Task OnInitializedAsync()
        {
            buglist = await mydatabaseService.GetBuglistById(Id);
            testcasesForTestcaseid = await mydatabaseService.GetTestcases();
        }

        protected async Task FormSubmit()
        {
            try
            {
                // Check if a new image was uploaded.
                if (uploadedFile != null)
                {
                    await SaveImageToServer();
                }

                // Update the database record.
                buglist.Updatedat = DateTime.Now;
                await mydatabaseService.UpdateBuglist(Id, buglist);
                DialogService.Close(buglist);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                Console.WriteLine($"Error updating Buglist: {ex.Message}");
                hasChanges = ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
                canEdit = !(ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException);
                errorVisible = true;
            }
        }

        protected void CancelButtonClick()
        {
            DialogService.Close(null);
        }

        protected async Task ReloadButtonClick()
        {
            mydatabaseService.Reset();
            hasChanges = false;
            canEdit = true;
            buglist = await mydatabaseService.GetBuglistById(Id);
            uploadedFile = null;
            imagePreview = null;
        }

        // Method to handle image selection from the file input.
        async Task OnImageSelected(InputFileChangeEventArgs args)
        {
            uploadedFile = args.File;

            if (uploadedFile != null)
            {
                var buffer = new byte[uploadedFile.Size];
                await uploadedFile.OpenReadStream().ReadAsync(buffer);
                // Create a temporary preview using Base64 encoding.
                imagePreview = $"data:image/png;base64,{Convert.ToBase64String(buffer)}";
            }
        }

        // Method to save the image to the server's file system.
        private async Task SaveImageToServer()
        {
            if (uploadedFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{uploadedFile.Name}";
                var savePath = Path.Combine("wwwroot/uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                // Read file stream and write to disk.
                using (var stream = uploadedFile.OpenReadStream())
                {
                    using (var fileStream = File.Create(savePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                // Update the buglist model with the new relative path.
                buglist.Image = $"/uploads/{fileName}";
            }
        }
    }
}