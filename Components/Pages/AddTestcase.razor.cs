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
    public partial class AddTestcase
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected DialogService DialogService { get; set; }
        [Inject] protected TooltipService TooltipService { get; set; }
        [Inject] protected ContextMenuService ContextMenuService { get; set; }
        [Inject] protected NotificationService NotificationService { get; set; }
        [Inject] public mydatabaseService mydatabaseService { get; set; }

        // Selected team members and their statuses (default Pending)
        protected Guid? selectedCoderId;
        protected TestStatus selectedCoderStatus = TestStatus.Pending;

        protected Guid? selectedTesterId;
        protected TestStatus selectedTesterStatus = TestStatus.Pending;

        protected Guid? selectedOwnerId;
        protected TestStatus selectedOwnerStatus = TestStatus.Pending;

       protected override async Task OnInitializedAsync()
{
    testcase = new Testcase();

    projectsForProjectid = await mydatabaseService.GetProjects();
    teammembersForCoder = (await mydatabaseService.GetTeammembers()).ToList();

    testStatusList = Enum.GetValues(typeof(TestStatus))
                         .Cast<TestStatus>()
                         .Select(ts => new KeyValuePair<TestStatus, string>(ts, ts.ToString()))
                         .ToList();

    // ✅ Set default team members if available
    if (teammembersForCoder.Any())
    {
        selectedCoderId = teammembersForCoder.ElementAtOrDefault(0)?.Id;
        selectedTesterId = teammembersForCoder.ElementAtOrDefault(1)?.Id;
        selectedOwnerId = teammembersForCoder.ElementAtOrDefault(2)?.Id;

        // Default all statuses to Pending
        selectedCoderStatus = TestStatus.Pending;
        selectedTesterStatus = TestStatus.Pending;
        selectedOwnerStatus = TestStatus.Pending;
    }
}


        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.Testcase testcase;
        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Project> projectsForProjectid;
        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember> teammembersForCoder;
        protected IEnumerable<KeyValuePair<TestStatus, string>> testStatusList;

        protected async Task FormSubmit()
{
    try
    {
        // Save the testcase first
        await mydatabaseService.CreateTestcase(testcase);

        // Get all team members from the database
        var allTeamMembers = (await mydatabaseService.GetTeammembers()).ToList();

        // Initialize TestcaseTeammember list
        var tecList = new List<TestcaseTeammember>();

        // Assign Coder (default: first team member)
        var coderId = selectedCoderId ?? allTeamMembers.ElementAtOrDefault(0)?.Id;
        if (coderId.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = coderId.Value,
                Role = Role.Coder,
                TestStatus = selectedCoderStatus
            });
        }

        // Assign Tester (default: second team member)
        var testerId = selectedTesterId ?? allTeamMembers.ElementAtOrDefault(1)?.Id;
        if (testerId.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = testerId.Value,
                Role = Role.Tester,
                TestStatus = selectedTesterStatus
            });
        }

        // Assign Owner (default: third team member)
        var ownerId = selectedOwnerId ?? allTeamMembers.ElementAtOrDefault(2)?.Id;
        if (ownerId.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = ownerId.Value,
                Role = Role.Owner,
                TestStatus = selectedOwnerStatus
            });
        }

        // Save all TestcaseTeammember records
        foreach (var t in tecList)
        {
            await mydatabaseService.CreateTestcaseTeammember(t);
        }

        // Create Buglist if any member has Fail or Issue status
        if (tecList.Any(t => t.TestStatus == TestStatus.Fail || t.TestStatus == TestStatus.Issue))
        {
            var buglistEntry = new Buglist
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
            };
            await mydatabaseService.CreateBuglist(buglistEntry);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Bug Created",
                Detail = "A new bug has been created in the buglist due to a failed test status."
            });
        }

        DialogService.Close(testcase);
    }
    catch (Exception ex)
    {
        errorVisible = true;
        Console.WriteLine(ex.Message);
    }
}


        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }

        protected bool hasChanges = false;
        protected bool canEdit = true;
    }
}
