using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace TestCaseDashboard.Components.Pages
{
    public partial class EditTestcase
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

        protected override async Task OnInitializedAsync()
        {
            testcase = await mydatabaseService.GetTestcaseById(Id);

            projectsForProjectid = await mydatabaseService.GetProjects();
        }
        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.Testcase testcase;

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Project> projectsForProjectid;

        protected async Task FormSubmit()
        {
            try
            {
                await mydatabaseService.UpdateTestcase(Id, testcase);
                DialogService.Close(testcase);
            }
            catch (Exception ex)
            {
                hasChanges = ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
                canEdit = !(ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException);
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }


        protected bool hasChanges = false;
        protected bool canEdit = true;


        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
           mydatabaseService.Reset();
            hasChanges = false;
            canEdit = true;

            testcase = await mydatabaseService.GetTestcaseById(Id);
        }
    }
}