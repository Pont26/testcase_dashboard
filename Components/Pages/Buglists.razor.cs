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
    public partial class Buglists
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

        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Buglist> buglists;

        protected RadzenDataGrid<TestCaseDashboard.Models.mydatabase.Buglist> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = (string)args.Value;

            await grid0.GoToPage(0);

            // Corrected: Filter on all relevant fields including navigation properties
            buglists = await mydatabaseService.GetBuglists(new Query { Filter = $@"i => i.Remark.Contains(@0) || i.Image.Contains(@0) || i.Testcase.Project.Name.Contains(@0) || i.Testcase.Screen.Contains(@0) || i.Testcase.Function.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Testcase.Project" });
        }

        protected override async Task OnInitializedAsync()
        {
            // Simplified: Load the data once on initialization with an expand for related properties
            buglists = await mydatabaseService.GetBuglists(new Query { Expand = "Testcase.Project" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddBuglist>("Add Buglist", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<TestCaseDashboard.Models.mydatabase.Buglist> args)
        {
            await DialogService.OpenAsync<EditBuglist>("Edit Buglist", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, TestCaseDashboard.Models.mydatabase.Buglist buglist)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await mydatabaseService.DeleteBuglist(buglist.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Buglist"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
                Filter = grid0.Query.Filter,
                OrderBy = grid0.Query.OrderBy,
                Expand = "Testcase,Testcase.Project", // Expanded to include both Testcase and Project
                Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property))
            };

            if (args?.Value == "csv")
            {
                await mydatabaseService.ExportBuglistsToCSV(query, "Buglists");
            }
            else if (args == null || args.Value == "xlsx")
            {
                await mydatabaseService.ExportBuglistsToExcel(query, "Buglists");
            }
        }
    }
}