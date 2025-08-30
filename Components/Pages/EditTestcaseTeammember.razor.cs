using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TestCaseDashboard.Models.mydatabase;
using Microsoft.EntityFrameworkCore;

namespace TestCaseDashboard.Components.Pages
{
    public partial class EditTestcaseTeammember
    {
        [Inject] protected DialogService DialogService { get; set; }
        [Inject] protected NotificationService NotificationService { get; set; }
        [Inject] public mydatabaseService mydatabaseService { get; set; }

        [Parameter] public Guid Id { get; set; }

        protected TestcaseTeammember testcaseTeammember;
        protected IEnumerable<Teammember> teammembersForTeammemberid;
        protected IEnumerable<KeyValuePair<TestStatus, string>> testStatusList;
        
        // This property is bound to the UI dropdown
        protected TestStatus selectedCoderStatus;

        // UI flags to match your HTML
        protected bool errorVisible = false;
        protected bool hasChanges = false;
        protected bool canEdit = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                testcaseTeammember = await mydatabaseService.GetTestcaseTeammemberById(Id);
                teammembersForTeammemberid = await mydatabaseService.GetTeammembers();
                
                // Populate the status dropdown
                testStatusList = Enum.GetValues(typeof(TestStatus)).Cast<TestStatus>()
                                       .Select(e => new KeyValuePair<TestStatus, string>(e, e.ToString())).ToList();
                
                // Initialize the UI dropdown with the current value
                selectedCoderStatus = testcaseTeammember.TestStatus;
            }
            catch (Exception ex)
            {
                // Handle cases where the item might be missing
                canEdit = false;
                errorVisible = true;
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = "Could not load test case team member." });
                Console.Error.WriteLine(ex.Message);
            }
        }

        protected async Task FormSubmit()
        {
            try
            {
                errorVisible = false;
                hasChanges = false;
                
                // Assign the dropdown's value back to the object's property before saving
                testcaseTeammember.TestStatus = selectedCoderStatus;

                await mydatabaseService.UpdateTestcaseTeammember(testcaseTeammember.Id, testcaseTeammember);

                // Check if the updated status requires a bug entry
                if (testcaseTeammember.TestStatus == TestStatus.Fail || testcaseTeammember.TestStatus == TestStatus.Issue)
                {
                    var testcase = await mydatabaseService.GetTestcaseById(testcaseTeammember.Testcaseid);
                    var buglistEntry = new Buglist
                    {
                        Id = Guid.NewGuid(),
                        Testcaseid = testcaseTeammember.Testcaseid,
                        Remark = $"Bug automatically generated for test case '{testcase.Screen}'. A team member reported a '{testcaseTeammember.TestStatus}' status."
                    };
                    await mydatabaseService.CreateBuglist(buglistEntry);
                    NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Info, Summary = "Bug Created", Detail = "A new bug has been created." });
                }
                
                DialogService.Close(testcaseTeammember);
            }
            catch (DbUpdateConcurrencyException)
            {
                hasChanges = true;
                canEdit = false;
                errorVisible = true;
            }
            catch (Exception ex)
            {
                errorVisible = true;
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = $"Unable to update record: {ex.Message}" });
            }
        }
        
        protected async Task ReloadButtonClick()
        {
            hasChanges = false;
            canEdit = true;
            await OnInitializedAsync();
        }

        protected void CancelButtonClick()
        {
            DialogService.Close(null);
        }
    }
}