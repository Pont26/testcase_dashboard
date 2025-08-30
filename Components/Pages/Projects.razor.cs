using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using TestCaseDashboard.Models.mydatabase;

namespace TestCaseDashboard.Components.Pages
{
    public partial class Projects
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

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Project> projects;

        protected RadzenDataGrid<TestCaseDashboard.Models.mydatabase.Project> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            projects = await mydatabaseService.GetProjects(new Query { Filter = $@"i => i.Projectname.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            projects = await mydatabaseService.GetProjects(new Query { Filter = $@"i => i.Projectname.Contains(@0)", FilterParameters = new object[] { search } });

        }

      protected async Task AddButtonClick(MouseEventArgs args)
{
    var result = await DialogService.OpenAsync<AddProject>("Add Project", null);

    if (result != null) // dialog closed with a saved project
    {
        projects = await mydatabaseService.GetProjects(new Query 
        { 
            Filter = $@"i => i.Projectname.Contains(@0)", 
            FilterParameters = new object[] { search } 
        });
        await grid0.Reload();
    }
}

protected async Task EditRow(Project args)
{
    var result = await DialogService.OpenAsync<EditProject>("Edit Project", 
        new Dictionary<string, object> { {"Id", args.Id} });

    if (result != null) // dialog closed with updated project
    {
        projects = await mydatabaseService.GetProjects(new Query 
        { 
            Filter = $@"i => i.Projectname.Contains(@0)", 
            FilterParameters = new object[] { search } 
        });
        await grid0.Reload();
    }
}

protected async Task GridDeleteButtonClick(MouseEventArgs args, Project project)
{
    try
    {
        if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
        {
            var deleteResult = await mydatabaseService.DeleteProject(project.Id);

            if (deleteResult != null)
            {
                projects = await mydatabaseService.GetProjects(new Query 
                { 
                    Filter = $@"i => i.Projectname.Contains(@0)", 
                    FilterParameters = new object[] { search } 
                });
                await grid0.Reload();
            }
        }
    }
    catch
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = $"Error",
            Detail = $"Unable to delete Project"
        });
    }
}

      
    }
}