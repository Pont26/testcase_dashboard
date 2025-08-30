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

        // Selected dropdown values
        protected Guid? selectedCoderId;
        protected Guid? selectedTesterId;
        protected Guid? selectedOwnerId;

        protected TestStatus? selectedCoderStatus;
        protected TestStatus? selectedTesterStatus;
        protected TestStatus? selectedOwnerStatus;

        // UI flags
        protected bool errorVisible = false;
        protected bool hasChanges = false;
        protected bool canEdit = true;
        protected bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;

            try
            {
                // Load testcase with related teammembers
                testcase = await mydatabaseService.GetTestcaseById(Id);

                // Load dropdown data
                projectsForProjectid = await mydatabaseService.GetProjects();
                teammembersForCoder = await mydatabaseService.GetTeammembers();

                testStatusList = Enum.GetValues(typeof(TestStatus))
                                     .Cast<TestStatus>()
                                     .Select(ts => new KeyValuePair<TestStatus, string>(ts, ts.ToString()))
                                     .ToList();

                // Initialize list if null
                if (testcase.TestcaseTeammembers == null)
                    testcase.TestcaseTeammembers = new List<TestcaseTeammember>();

                // Prefill dropdowns from existing assignments
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
                Console.Error.WriteLine($"[EditTestcaseBase] Load error: {ex}");
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
                errorVisible = false;
                testcase.Updatedat = DateTime.UtcNow;

                // Reset coder/tester/owner associations
                testcase.TestcaseTeammembers = new List<TestcaseTeammember>();

                if (selectedCoderId.HasValue)
                {
                    testcase.TestcaseTeammembers.Add(new TestcaseTeammember
                    {
                        Teammemberid = selectedCoderId.Value,
                        Role = Role.Coder,
                        TestStatus = selectedCoderStatus ?? TestStatus.Pending
                    });
                }

                if (selectedTesterId.HasValue)
                {
                    testcase.TestcaseTeammembers.Add(new TestcaseTeammember
                    {
                        Teammemberid = selectedTesterId.Value,
                        Role = Role.Tester,
                        TestStatus = selectedTesterStatus ?? TestStatus.Pending
                    });
                }

                if (selectedOwnerId.HasValue)
                {
                    testcase.TestcaseTeammembers.Add(new TestcaseTeammember
                    {
                        Teammemberid = selectedOwnerId.Value,
                        Role = Role.Owner,
                        TestStatus = selectedOwnerStatus ?? TestStatus.Pending
                    });
                }

                await mydatabaseService.UpdateTestcase(testcase.Id, testcase);
                DialogService.Close(testcase);
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
                Console.Error.WriteLine($"[EditTestcaseBase] Save error: {ex}");
            }
        }

        protected void CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}
