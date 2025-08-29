using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using TestCaseDashboard.Models.mydatabase;
using Microsoft.EntityFrameworkCore;

namespace TestCaseDashboard.Components.Pages
{
    public class EditTestcaseBase : ComponentBase
    {
        [Inject] protected DialogService DialogService { get; set; }
        [Inject] public mydatabaseService mydatabaseService { get; set; }

        [Parameter] public Guid Id { get; set; }

        protected Testcase testcase;
        protected IEnumerable<Project> projectsForProjectid;
        protected IEnumerable<Teammember> teammembersForCoder;
        protected IEnumerable<KeyValuePair<TestStatus, string>> testStatusList;

        protected Guid? selectedCoderId;
        protected Guid? selectedTesterId;
        protected Guid? selectedOwnerId;

        protected TestStatus? selectedCoderStatus;
        protected TestStatus? selectedTesterStatus;
        protected TestStatus? selectedOwnerStatus;

        protected bool errorVisible = false;
        protected bool hasChanges = false;
        protected bool canEdit = true;
        protected bool isLoading = true; // Add this loading flag

       protected override async Task OnInitializedAsync()
{
    isLoading = true; 
    try
    {
        // Load the testcase (including existing TestcaseTeammembers)
        testcase = await mydatabaseService.GetTestcaseById(Id);

        // Load dropdown data
        projectsForProjectid = await mydatabaseService.GetProjects();
        teammembersForCoder = await mydatabaseService.GetTeammembers();
        testStatusList = Enum.GetValues(typeof(TestStatus))
                               .Cast<TestStatus>()
                               .Select(ts => new KeyValuePair<TestStatus, string>(ts, ts.ToString()))
                               .ToList();

        if (testcase.TestcaseTeammembers == null)
            testcase.TestcaseTeammembers = new List<TestcaseTeammember>();

        // Pre-fill dropdowns from existing data
        var coder = testcase.TestcaseTeammembers.FirstOrDefault(t => t.Role == Role.Coder);
        selectedCoderId = coder?.Teammemberid;
        selectedCoderStatus = coder?.TestStatus;

        var tester = testcase.TestcaseTeammembers.FirstOrDefault(t => t.Role == Role.Tester);
        selectedTesterId = tester?.Teammemberid;
        selectedTesterStatus = tester?.TestStatus;

        var owner = testcase.TestcaseTeammembers.FirstOrDefault(t => t.Role == Role.Owner);
        selectedOwnerId = owner?.Teammemberid;
        selectedOwnerStatus = owner?.TestStatus;
    }
    catch (Exception ex)
    {
        errorVisible = true;
        // optionally log ex
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}

       protected async Task FormSubmit()
{
    try
    {
        // Update coder
        UpdateTeamMemberSelection(Role.Coder, selectedCoderId, selectedCoderStatus);

        // Update tester
        UpdateTeamMemberSelection(Role.Tester, selectedTesterId, selectedTesterStatus);

        // Update owner
        UpdateTeamMemberSelection(Role.Owner, selectedOwnerId, selectedOwnerStatus);

        // Update testcase (this also updates TestcaseTeammembers in service)
        await mydatabaseService.UpdateTestcase(testcase.Id, testcase);

        DialogService.Close(testcase);
    }
    catch (DbUpdateConcurrencyException)
    {
        hasChanges = true;
        canEdit = false;
        errorVisible = true;
    }
    catch (Exception)
    {
        errorVisible = true;
    }
}

private void UpdateTeamMemberSelection(Role role, Guid? memberId, TestStatus? status)
{
    var existing = testcase.TestcaseTeammembers
                           .FirstOrDefault(x => x.Role == role);

    if (existing != null)
    {
        if (memberId.HasValue && status.HasValue)
        {
            existing.Teammemberid = memberId.Value;
            existing.TestStatus = status.Value;
        }
        // If memberId is null, you could decide to leave as-is or clear values
    }
    // else: do nothing (no creation)
}

        protected void CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}