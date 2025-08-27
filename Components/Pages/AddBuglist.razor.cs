using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using TestCaseDashboard.Models.mydatabase;

namespace TestCaseDashboard.Components.Pages
{
    public partial class AddBuglist
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected DialogService DialogService { get; set; }
        [Inject] protected TooltipService TooltipService { get; set; }
        [Inject] protected ContextMenuService ContextMenuService { get; set; }
        [Inject] protected NotificationService NotificationService { get; set; }
        [Inject] public mydatabaseService mydatabaseService { get; set; }

        protected bool errorVisible;
        protected Buglist buglist;
        protected IEnumerable<Testcase> testcasesForTestcaseid;

        private string imagePreview;
        private IBrowserFile uploadedFile;

        protected override async Task OnInitializedAsync()
        {
            buglist = new Buglist
            {
                Createdat = DateTime.Now,
                Updatedat = DateTime.Now
            };

            testcasesForTestcaseid = await mydatabaseService.GetTestcases();
        }

        protected async Task FormSubmit()
        {
            try
            {
                if (uploadedFile != null)
                {
                    await SaveImageToServer();
                }

                await mydatabaseService.CreateBuglist(buglist);
                DialogService.Close(buglist);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Buglist: {ex.Message}");
                errorVisible = true;
            }
        }

        protected void CancelButtonClick()
        {
            DialogService.Close(null);
        }

        async Task OnImageSelected(InputFileChangeEventArgs args)
        {
            uploadedFile = args.File;

            if (uploadedFile != null)
            {
                var buffer = new byte[uploadedFile.Size];
                await uploadedFile.OpenReadStream().ReadAsync(buffer);
                imagePreview = $"data:image/png;base64,{Convert.ToBase64String(buffer)}";
            }
        }

        private async Task SaveImageToServer()
        {
            if (uploadedFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{uploadedFile.Name}";
                var savePath = Path.Combine("wwwroot/uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                using (var stream = uploadedFile.OpenReadStream())
                {
                    using (var fileStream = File.Create(savePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                buglist.Image = $"/uploads/{fileName}";
            }
        }
    }
}