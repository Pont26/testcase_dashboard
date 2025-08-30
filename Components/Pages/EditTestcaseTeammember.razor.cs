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
    public partial class EditTestcaseTeammember
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
            testcaseTeammember = await mydatabaseService.GetTestcaseTeammemberById(Id);
            teammembersForTeammemberid = await mydatabaseService.GetTeammembers();
            testcasesForTestcaseid = await mydatabaseService.GetTestcases();

            // Populate the dropdown list from the TestStatus enum
            testStatusList = Enum.GetValues(typeof(TestStatus))
                                 .Cast<TestStatus>()
                                 .Select(e => new KeyValuePair<TestStatus, string>(e, e.ToString()))
                                 .ToList();

            // Set the value of the dropdown from the existing object property
            selectedCoderStatus = testcaseTeammember.TestStatus;
        }

        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseTeammember;
        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember> teammembersForTeammemberid;
        protected IEnumerable<TestCaseDashboard.Models.mydatabase.Testcase> testcasesForTestcaseid;
        protected IEnumerable<KeyValuePair<TestStatus, string>> testStatusList;
        
        // Define the missing property
        protected TestStatus selectedCoderStatus;

        protected async Task FormSubmit()
        {
            try
            {
                // Update the TestStatus on the object before saving
                testcaseTeammember.TestStatus = selectedCoderStatus;
                await mydatabaseService.UpdateTestcaseTeammember(Id, testcaseTeammember);
                DialogService.Close(testcaseTeammember);
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
            testcaseTeammember = await mydatabaseService.GetTestcaseTeammemberById(Id);
            selectedCoderStatus = testcaseTeammember.TestStatus;
        }
    }
}