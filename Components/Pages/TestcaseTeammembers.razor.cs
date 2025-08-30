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
    public partial class TestcaseTeammembers
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

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> testcaseTeammembers;

        protected RadzenDataGrid<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            testcaseTeammembers = await mydatabaseService.GetTestcaseTeammembers(new Query { Expand = "Teammember,Testcase" });
        }
        protected override async Task OnInitializedAsync()
        {
            testcaseTeammembers = await mydatabaseService.GetTestcaseTeammembers(new Query { Expand = "Teammember,Testcase,Testcase.Project" });
        }

       

        protected async Task EditRow(TestCaseDashboard.Models.mydatabase.TestcaseTeammember args)
        {
           var result =  await DialogService.OpenAsync<EditTestcaseTeammember>("Edit TestcaseTeammember", new Dictionary<string, object> { {"Id", args.Id} });
           if(result!=null){
           LoadTestcaseTeammembers();
           }
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseTeammember)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await mydatabaseService.DeleteTestcaseTeammember(testcaseTeammember.Id);

                    if (deleteResult != null)
                    {
                        LoadTestcaseTeammembers();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete TestcaseTeammember"
                });
            }
        }

        private async Task LoadTestcaseTeammembers()
{
    testcaseTeammembers = await mydatabaseService.GetTestcaseTeammembers(
        new Query 
        { 
            Expand = "Teammember,Testcase,Testcase.Project" 
        });
    await grid0.Reload();
}

       
    }
}