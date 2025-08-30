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
    public partial class Teammembers
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

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember> teammembers;

        protected RadzenDataGrid<TestCaseDashboard.Models.mydatabase.Teammember> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            teammembers = await mydatabaseService.GetTeammembers(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            teammembers = await mydatabaseService.GetTeammembers(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
             var result = await DialogService.OpenAsync<AddTeammember>("Add Teammember", null);
    if (result != null)
    {
        await LoadTeammembers(); // ✅ refresh only if saved
    }
        }

        protected async Task EditRow(TestCaseDashboard.Models.mydatabase.Teammember args)
        {
            var result = await DialogService.OpenAsync<EditTeammember>("Edit Teammember", new Dictionary<string, object> { {"Id", args.Id} });
    if (result != null)
    {
        await LoadTeammembers(); // ✅ refresh only if saved
    }

        }

       protected async Task GridDeleteButtonClick(MouseEventArgs args, TestCaseDashboard.Models.mydatabase.Teammember teammember)
{
    try
    {
        if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
        {
            var deleteResult = await mydatabaseService.DeleteTeammember(teammember.Id);

            if (deleteResult != null)
            {
                await LoadTeammembers(); // ✅ awaited
            }
        }
    }
    catch (Exception)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = $"Error",
            Detail = $"Unable to delete Teammember"
        });
    }
}

private async Task LoadTeammembers()
{
    teammembers = await mydatabaseService.GetTeammembers(
        new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
    await grid0.Reload();
}

        

      
    }
}