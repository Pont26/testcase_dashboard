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
    public partial class Testcases
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

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Testcase> testcases;

        protected RadzenDataGrid<TestCaseDashboard.Models.mydatabase.Testcase> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            testcases = await mydatabaseService.GetTestcases(new Query { Filter = $@"i => i.Screen.Contains(@0) || i.Function.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Project" });
        }
       protected override async Task OnInitializedAsync()
{
    await LoadTestcase(); // reuse helper
}

protected async Task AddButtonClick(MouseEventArgs args)
{
    var result = await DialogService.OpenAsync<AddTestcase>("Add Testcase", null);
    if(result != null){
        await LoadTestcase(); // ✅ awaited
    }
}

protected async Task EditRow(TestCaseDashboard.Models.mydatabase.Testcase args)
{
   var result = await DialogService.OpenAsync<EditTestcase>("Edit Testcase", new Dictionary<string, object> { {"Id", args.Id} });
   if(result != null){
        await LoadTestcase(); // ✅ awaited
   }
}

protected async Task GridDeleteButtonClick(MouseEventArgs args, TestCaseDashboard.Models.mydatabase.Testcase testcase)
{
    try
    {
        if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
        {
            var deleteResult = await mydatabaseService.DeleteTestcase(testcase.Id);

            if (deleteResult != null)
            {
                await LoadTestcase(); // ✅ awaited
            }
        }
    }
    catch (Exception)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = $"Error",
            Detail = $"Unable to delete Testcase"
        });
    }
}

private async Task LoadTestcase()
{
    testcases = await mydatabaseService.GetTestcases(
        new Query { 
            Filter = $@"i => i.Screen.Contains(@0) || i.Function.Contains(@0)",
            FilterParameters = new object[] { search },
            Expand = "Project,TestcaseTeammembers"
        });
    await grid0.Reload();
}



     
    }
}