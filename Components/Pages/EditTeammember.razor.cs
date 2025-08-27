using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using TestCaseDashboard.Models.mydatabase;

namespace TestCaseDashboard.Components.Pages
{
    public partial class EditTeammember
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected DialogService DialogService { get; set; }
        [Inject] protected TooltipService TooltipService { get; set; }
        [Inject] protected ContextMenuService ContextMenuService { get; set; }
        [Inject] protected NotificationService NotificationService { get; set; }
        [Inject] public mydatabaseService mydatabaseService { get; set; }

        [Parameter] public Guid Id { get; set; }

        protected Teammember teammember;
        protected bool errorVisible = false;
        protected bool hasChanges = false;
        protected bool canEdit = true;

        protected override async Task OnInitializedAsync()
        {
            teammember = await mydatabaseService.GetTeammemberById(Id);
        }

        protected async Task FormSubmit()
        {
            try
            {
                await mydatabaseService.UpdateTeammember(Id, teammember);
                DialogService.Close(teammember); // close modal and return updated object
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

        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
            mydatabaseService.Reset();
            hasChanges = false;
            canEdit = true;
            teammember = await mydatabaseService.GetTeammemberById(Id);
        }
    }
}
