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
    public partial class AddTeammember
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

        protected override async Task OnInitializedAsync()
        {
            teammember = new TestCaseDashboard.Models.mydatabase.Teammember();
        }
        protected bool errorVisible;
        protected TestCaseDashboard.Models.mydatabase.Teammember teammember;

        protected async Task FormSubmit()
        {
            try
            {
                await mydatabaseService.CreateTeammember(teammember);
                StateHasChanged();
                DialogService.Close(teammember);
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
    }
}