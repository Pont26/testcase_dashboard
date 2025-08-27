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
    public partial class EditProjectTeammember
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
            projectTeammember = await mydatabaseService.GetProjectTeammemberById(Id);

            teammembersForTeammemberid = await mydatabaseService.GetTeammembers();

            projectsForProjectid = await mydatabaseService.GetProjects();
        }
        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.ProjectTeammember projectTeammember;

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember> teammembersForTeammemberid;

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Project> projectsForProjectid;

        protected async Task FormSubmit()
        {
            try
            {
                await mydatabaseService.UpdateProjectTeammember(Id, projectTeammember);
                DialogService.Close(projectTeammember);
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

            projectTeammember = await mydatabaseService.GetProjectTeammemberById(Id);
        }
    }
}