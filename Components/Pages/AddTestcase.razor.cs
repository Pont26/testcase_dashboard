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

        // A property to hold the selected coder's ID
        protected Guid? selectedTeammemberId;
       protected TestStatus? selectedTestStatus;
       protected Guid? selectedCoderId;
protected TestStatus? selectedCoderStatus;

protected Guid? selectedTesterId;
protected TestStatus? selectedTesterStatus;

protected Guid? selectedOwnerId;
protected TestStatus? selectedOwnerStatus;


        protected override async Task OnInitializedAsync()
        {
            testcase = new TestCaseDashboard.Models.mydatabase.Testcase();
            projectsForProjectid = await mydatabaseService.GetProjects();
            teammembersForCoder = await mydatabaseService.GetTeammembers(); 
             testStatusList = Enum.GetValues(typeof(TestStatus))
                         .Cast<TestStatus>()
                         .Select(ts => new KeyValuePair<TestStatus, string>(ts, ts.ToString()))
                         .ToList();
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

        // Create Teammembers
        var tecList = new List<TestcaseTeammember>();

        if (selectedCoderId.HasValue && selectedCoderStatus.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = selectedCoderId.Value,
                Role = Role.Coder,
                TestStatus = selectedCoderStatus.Value
            });
        }

        if (selectedTesterId.HasValue && selectedTesterStatus.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = selectedTesterId.Value,
                Role = Role.Tester,
                TestStatus = selectedTesterStatus.Value
            });
        }

        if (selectedOwnerId.HasValue && selectedOwnerStatus.HasValue)
        {
            tecList.Add(new TestcaseTeammember
            {
                Id = Guid.NewGuid(),
                Testcaseid = testcase.Id,
                Teammemberid = selectedOwnerId.Value,
                Role = Role.Owner,
                TestStatus = selectedOwnerStatus.Value
            });
        }

        // Save all TestcaseTeammember records
        foreach (var t in tecList)
        {
            await mydatabaseService.CreateTestcaseTeammember(t);
        }

        DialogService.Close(testcase);
    }
    catch (Exception ex)
    {
        errorVisible = true;
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