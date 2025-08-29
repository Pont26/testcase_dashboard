using System;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

using TestCaseDashboard.Data;

namespace TestCaseDashboard
{
    public partial class mydatabaseService
    {
        mydatabaseContext Context
        {
           get
           {
             return this.context;
           }
        }

        private readonly mydatabaseContext context;
        private readonly NavigationManager navigationManager;

        public mydatabaseService(mydatabaseContext context, NavigationManager navigationManager)
        {
            this.context = context;
            this.navigationManager = navigationManager;
        }

        public void Reset() => Context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public void ApplyQuery<T>(ref IQueryable<T> items, Query query = null)
        {
            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    if (query.FilterParameters != null)
                    {
                        items = items.Where(query.Filter, query.FilterParameters);
                    }
                    else
                    {
                        items = items.Where(query.Filter);
                    }
                }

                if (!string.IsNullOrEmpty(query.OrderBy))
                {
                    items = items.OrderBy(query.OrderBy);
                }

                if (query.Skip.HasValue)
                {
                    items = items.Skip(query.Skip.Value);
                }

                if (query.Top.HasValue)
                {
                    items = items.Take(query.Top.Value);
                }
            }
        }


        public async Task ExportBuglistsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/buglists/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/buglists/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportBuglistsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/buglists/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/buglists/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnBuglistsRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Buglist> items);

        public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.Buglist>> GetBuglists(Query query = null)
        {
            var items = Context.Buglists.AsQueryable();

            items = items.Include(i => i.TestcaseTeammember);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnBuglistsRead(ref items);

            return await Task.FromResult(items);
        }

       partial void OnBuglistGet(TestCaseDashboard.Models.mydatabase.Buglist item);
        partial void OnGetBuglistById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Buglist> items);


        public async Task<TestCaseDashboard.Models.mydatabase.Buglist> GetBuglistById(Guid id)
        {
            var items = Context.Buglists
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.TestcaseTeammember);
 
            OnGetBuglistById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnBuglistGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnBuglistCreated(TestCaseDashboard.Models.mydatabase.Buglist item);
        partial void OnAfterBuglistCreated(TestCaseDashboard.Models.mydatabase.Buglist item);

        public async Task<TestCaseDashboard.Models.mydatabase.Buglist> CreateBuglist(TestCaseDashboard.Models.mydatabase.Buglist buglist)
{
    OnBuglistCreated(buglist);

    // The logic to check for an existing item is removed.
    // A 'Create' method should assume it's dealing with a new item.
    OnBuglistCreated(buglist);
    var existingItem = Context.Buglists
                              .Where(i => i.Id == buglist.Id)
                              .FirstOrDefault();
      if (existingItem != null)
            {
               throw new Exception("Item already available");
            }   

    try
    {
        // Add the new buglist item to the context.
        Context.Buglists.Add(buglist);

        // Set the creation and update timestamps.
        // It's good practice to set a 'CreatedAt' timestamp as well.
        buglist.Createdat = DateTime.UtcNow;
        buglist.Updatedat = DateTime.UtcNow;

        // Save the changes to the database.
        await Context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        // Detach the entity from the context to prevent it from being tracked
        // and re-throw the exception to be handled by the calling method.
        Context.Entry(buglist).State = EntityState.Detached;
        throw;
    }

    OnAfterBuglistCreated(buglist);

    return buglist;
}
      public async Task<TestCaseDashboard.Models.mydatabase.Buglist> CancelBuglistChanges(TestCaseDashboard.Models.mydatabase.Buglist item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnBuglistUpdated(TestCaseDashboard.Models.mydatabase.Buglist item);
        partial void OnAfterBuglistUpdated(TestCaseDashboard.Models.mydatabase.Buglist item);

        public async Task<TestCaseDashboard.Models.mydatabase.Buglist> UpdateBuglist(Guid id, TestCaseDashboard.Models.mydatabase.Buglist buglist)
        {
            OnBuglistUpdated(buglist);

            var itemToUpdate = Context.Buglists
                              .Where(i => i.Id == buglist.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
             itemToUpdate.Remark = buglist.Remark;
             itemToUpdate.Image = buglist.Image;  
            itemToUpdate.Updatedat = DateTime.UtcNow;
 
        

            await Context.SaveChangesAsync();

             return itemToUpdate;
        }

        partial void OnBuglistDeleted(TestCaseDashboard.Models.mydatabase.Buglist item);
        partial void OnAfterBuglistDeleted(TestCaseDashboard.Models.mydatabase.Buglist item);

        public async Task<TestCaseDashboard.Models.mydatabase.Buglist> DeleteBuglist(Guid id)
        {
            var itemToDelete = Context.Buglists
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnBuglistDeleted(itemToDelete);


            Context.Buglists.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterBuglistDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportProjectsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/projects/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/projects/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportProjectsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/projects/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/projects/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnProjectsRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Project> items);

        public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.Project>> GetProjects(Query query = null)
        {
            var items = Context.Projects.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnProjectsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnProjectGet(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnGetProjectById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Project> items);


        public async Task<TestCaseDashboard.Models.mydatabase.Project> GetProjectById(Guid id)
        {
            var items = Context.Projects
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetProjectById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnProjectGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnProjectCreated(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnAfterProjectCreated(TestCaseDashboard.Models.mydatabase.Project item);

        public async Task<TestCaseDashboard.Models.mydatabase.Project> CreateProject(TestCaseDashboard.Models.mydatabase.Project project)
{
    OnProjectCreated(project);
    var existingItem = await Context.Projects.FirstOrDefaultAsync(i=> i.Id == project.Id);
    if(existingItem != null){
        throw new Exception("Item already available");
    }

    try
    {
        // Automatically set the creation and update timestamps
        var now = DateTime.UtcNow;
        project.Createdat = now;
        project.Updatedat = now;

        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
    }
    catch
    {
        Context.Entry(project).State = EntityState.Detached;
        throw;
    }

    OnAfterProjectCreated(project);

    return project;
}

        public async Task<TestCaseDashboard.Models.mydatabase.Project> CancelProjectChanges(TestCaseDashboard.Models.mydatabase.Project item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnProjectUpdated(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnAfterProjectUpdated(TestCaseDashboard.Models.mydatabase.Project item);

      public async Task<TestCaseDashboard.Models.mydatabase.Project> UpdateProject(Guid id, TestCaseDashboard.Models.mydatabase.Project project)
{
    // Find existing item by id using the async method
    var itemToUpdate = await Context.Projects
                                    .FirstOrDefaultAsync(i => i.Id == id);

    if (itemToUpdate == null)
        throw new Exception("Item no longer available");

    // Update only the specific properties you need
    itemToUpdate.Projectname = project.Projectname;
    itemToUpdate.ProjectSource = project.ProjectSource;
    itemToUpdate.Updatedat = DateTime.UtcNow;

    // Save changes asynchronously
    await Context.SaveChangesAsync();

    return itemToUpdate;
}

        partial void OnProjectDeleted(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnAfterProjectDeleted(TestCaseDashboard.Models.mydatabase.Project item);

        public async Task<TestCaseDashboard.Models.mydatabase.Project> DeleteProject(Guid id)
        {
            var itemToDelete = Context.Projects
                              .Where(i => i.Id == id)
                              .Include(i => i.ProjectTeammembers)
                              .Include(i => i.Testcases)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnProjectDeleted(itemToDelete);


            Context.Projects.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterProjectDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportProjectTeammembersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/projectteammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/projectteammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportProjectTeammembersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/projectteammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/projectteammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnProjectTeammembersRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember> items);

        public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember>> GetProjectTeammembers(Query query = null)
        {
            var items = Context.ProjectTeammembers.AsQueryable();

            items = items.Include(i => i.Project);
            items = items.Include(i => i.Teammember);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnProjectTeammembersRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnProjectTeammemberGet(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnGetProjectTeammemberById(ref IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember> items);


        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> GetProjectTeammemberById(Guid id)
        {
            var items = Context.ProjectTeammembers
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Project);
            items = items.Include(i => i.Teammember);
 
            OnGetProjectTeammemberById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnProjectTeammemberGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnProjectTeammemberCreated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberCreated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> CreateProjectTeammember(TestCaseDashboard.Models.mydatabase.ProjectTeammember projectteammember)
        {
            OnProjectTeammemberCreated(projectteammember);

            var existingItem = Context.ProjectTeammembers
                              .Where(i => i.Id == projectteammember.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.ProjectTeammembers.Add(projectteammember);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(projectteammember).State = EntityState.Detached;
                throw;
            }

            OnAfterProjectTeammemberCreated(projectteammember);

            return projectteammember;
        }

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> CancelProjectTeammemberChanges(TestCaseDashboard.Models.mydatabase.ProjectTeammember item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnProjectTeammemberUpdated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberUpdated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> UpdateProjectTeammember(Guid id, TestCaseDashboard.Models.mydatabase.ProjectTeammember projectteammember)
        {
            OnProjectTeammemberUpdated(projectteammember);

            var itemToUpdate = Context.ProjectTeammembers
                              .Where(i => i.Id == projectteammember.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(projectteammember);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterProjectTeammemberUpdated(projectteammember);

            return projectteammember;
        }

        partial void OnProjectTeammemberDeleted(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberDeleted(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> DeleteProjectTeammember(Guid id)
        {
            var itemToDelete = Context.ProjectTeammembers
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnProjectTeammemberDeleted(itemToDelete);


            Context.ProjectTeammembers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterProjectTeammemberDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportTeammembersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/teammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/teammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportTeammembersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/teammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/teammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnTeammembersRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Teammember> items);
public async Task<IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember>> GetTeammembers(Query query = null)
{
    var items = Context.Teammembers.AsQueryable();

    if (query != null)
    {
        if (!string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach (var p in propertiesToExpand)
            {
                items = items.Include(p.Trim());
            }
        }

        // Apply any filters from the query object
        ApplyQuery(ref items, query);
    }

    // Now, execute the query asynchronously and return the list.
    return await items.ToListAsync();
}

        partial void OnTeammemberGet(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnGetTeammemberById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Teammember> items);


        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> GetTeammemberById(Guid id)
        {
            var items = Context.Teammembers
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetTeammemberById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnTeammemberGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnTeammemberCreated(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnAfterTeammemberCreated(TestCaseDashboard.Models.mydatabase.Teammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> CreateTeammember(
    TestCaseDashboard.Models.mydatabase.Teammember teammember)
{
    OnTeammemberCreated(teammember);

    var existingItem = await Context.Teammembers
                              .FirstOrDefaultAsync(i => i.Id == teammember.Id);

    if (existingItem != null)
    {
       throw new Exception("Item already available");
    }

    try
    {
        var now = DateTime.UtcNow;
        teammember.Createdat = now;   // make sure property name matches DB
        teammember.Updatedat = now;

        Context.Teammembers.Add(teammember);
        await Context.SaveChangesAsync();
    }
    catch
    {
        Context.Entry(teammember).State = EntityState.Detached;
        throw;
    }

    OnAfterTeammemberCreated(teammember);

    return teammember;
}


        

        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> CancelTeammemberChanges(TestCaseDashboard.Models.mydatabase.Teammember item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnTeammemberUpdated(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnAfterTeammemberUpdated(TestCaseDashboard.Models.mydatabase.Teammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> UpdateTeammember(
    Guid id, TestCaseDashboard.Models.mydatabase.Teammember teammember)
{
    // Find existing item by id
    var itemToUpdate = await Context.Teammembers
                                .FirstOrDefaultAsync(i => i.Id == id);

    if (itemToUpdate == null)
        throw new Exception("Item no longer available");

    // Update only the Name and UpdatedAt
    itemToUpdate.Name = teammember.Name;
    itemToUpdate.Updatedat = DateTime.UtcNow;

    // Save changes asynchronously
    await Context.SaveChangesAsync();

    return itemToUpdate;
}


        partial void OnTeammemberDeleted(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnAfterTeammemberDeleted(TestCaseDashboard.Models.mydatabase.Teammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> DeleteTeammember(Guid id)
        {
            var itemToDelete = Context.Teammembers
                              .Where(i => i.Id == id)
                              .Include(i => i.ProjectTeammembers)
                              .Include(i => i.TestcaseTeammembers)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnTeammemberDeleted(itemToDelete);


            Context.Teammembers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterTeammemberDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportTestcasesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/testcases/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/testcases/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportTestcasesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/testcases/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/testcases/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnTestcasesRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Testcase> items);

        public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.Testcase>> GetTestcases(Query query = null)
{
    // Include Project and TestcaseTeammembers with Teammember
    var items = Context.Testcases
                       .Include(t => t.Project)
                       .Include(t => t.TestcaseTeammembers)
                           .ThenInclude(tm => tm.Teammember)
                       .AsQueryable();

    if (query != null)
    {
        if (!string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach (var p in propertiesToExpand)
            {
                items = items.Include(p.Trim());
            }
        }

        ApplyQuery(ref items, query);
    }

    OnTestcasesRead(ref items);

    return await Task.FromResult(items);
}

        partial void OnTestcaseGet(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnGetTestcaseById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Testcase> items);



public async Task<TestCaseDashboard.Models.mydatabase.Testcase> GetTestcaseById(Guid id)
{
return await Context.Testcases
    .Include(t => t.TestcaseTeammembers)
        .ThenInclude(tm => tm.Teammember) // optional, for names etc.
    .FirstOrDefaultAsync(t => t.Id == id);

}

        partial void OnTestcaseCreated(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseCreated(TestCaseDashboard.Models.mydatabase.Testcase item);

        public async Task<TestCaseDashboard.Models.mydatabase.Testcase> CreateTestcase(TestCaseDashboard.Models.mydatabase.Testcase testcase)
{
    OnTestcaseCreated(testcase);

    var existingItem = Context.Testcases
                              .Where(i => i.Id == testcase.Id)
                              .FirstOrDefault();

    if (existingItem != null)
    {
        throw new Exception("Item already available");
    }

    // Automatically set UTC timestamps
    if (testcase.Createdat == default)
        testcase.Createdat = DateTime.UtcNow;

    testcase.Updatedat = DateTime.UtcNow;

    try
    {
        Context.Testcases.Add(testcase);
        Context.SaveChanges();
    }
    catch
    {
        Context.Entry(testcase).State = EntityState.Detached;
        throw;
    }

    OnAfterTestcaseCreated(testcase);

    return testcase;
}

        partial void OnTestcaseUpdated(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseUpdated(TestCaseDashboard.Models.mydatabase.Testcase item);

public async Task<TestCaseDashboard.Models.mydatabase.Testcase> UpdateTestcase(Guid id, TestCaseDashboard.Models.mydatabase.Testcase testcase)
{
    // Include existing team members
    var itemToUpdate = await Context.Testcases
                                    .Include(t => t.TestcaseTeammembers)
                                    .FirstOrDefaultAsync(i => i.Id == id);

    if (itemToUpdate == null)
    {
        throw new Exception("Testcase no longer available");
    }

    // Update main testcase fields
    itemToUpdate.Screen = testcase.Screen;
    itemToUpdate.Function = testcase.Function;
    itemToUpdate.Projectid = testcase.Projectid;
    itemToUpdate.Updatedat = DateTime.UtcNow;

    // Update existing team members
    if (testcase.TestcaseTeammembers != null)
    {
        foreach (var updatedMember in testcase.TestcaseTeammembers)
        {
            var existing = itemToUpdate.TestcaseTeammembers
                                       .FirstOrDefault(x => x.Id == updatedMember.Id);

            if (existing != null)
            {
                existing.Teammemberid = updatedMember.Teammemberid;
                existing.Role = updatedMember.Role;
                existing.TestStatus = updatedMember.TestStatus;
            }
            // If member doesn't exist, we skip it (do not create new)
        }
    }

    await Context.SaveChangesAsync();

    return itemToUpdate;
}







        partial void OnTestcaseDeleted(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseDeleted(TestCaseDashboard.Models.mydatabase.Testcase item);

        public async Task<TestCaseDashboard.Models.mydatabase.Testcase> DeleteTestcase(Guid id)
        {
            var itemToDelete = Context.Testcases
                              .Where(i => i.Id == id)
                              .Include(i => i.TestcaseTeammembers)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnTestcaseDeleted(itemToDelete);


            Context.Testcases.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterTestcaseDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportTestcaseTeammembersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/testcaseteammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/testcaseteammembers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportTestcaseTeammembersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/mydatabase/testcaseteammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/mydatabase/testcaseteammembers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnTestcaseTeammembersRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> items);

       public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.TestcaseTeammember>> GetTestcaseTeammembers(Query query = null)
{
    var items = Context.TestcaseTeammembers.AsQueryable();

    items = items.Include(i => i.Teammember);
    items = items.Include(i => i.Testcase);
    items = items.Include(i => i.Testcase.Project); // Add this line

    if (query != null)
    {
        if (!string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach(var p in propertiesToExpand)
            {
                items = items.Include(p.Trim());
            }
        }

        ApplyQuery(ref items, query);
    }

    OnTestcaseTeammembersRead(ref items);

    return await Task.FromResult(items);
}
        partial void OnTestcaseTeammemberGet(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnGetTestcaseTeammemberById(ref IQueryable<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> items);


        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> GetTestcaseTeammemberById(Guid id)
        {
            var items = Context.TestcaseTeammembers
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Teammember);
            items = items.Include(i => i.Testcase);
 
            OnGetTestcaseTeammemberById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnTestcaseTeammemberGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnTestcaseTeammemberCreated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberCreated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> CreateTestcaseTeammember(TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseteammember)
        {
            OnTestcaseTeammemberCreated(testcaseteammember);

            var existingItem = Context.TestcaseTeammembers
                              .Where(i => i.Id == testcaseteammember.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.TestcaseTeammembers.Add(testcaseteammember);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(testcaseteammember).State = EntityState.Detached;
                throw;
            }

            OnAfterTestcaseTeammemberCreated(testcaseteammember);

            return testcaseteammember;
        }

        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> CancelTestcaseTeammemberChanges(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnTestcaseTeammemberUpdated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberUpdated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> UpdateTestcaseTeammember(Guid id, TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseteammember)
        {
            OnTestcaseTeammemberUpdated(testcaseteammember);

            var itemToUpdate = Context.TestcaseTeammembers
                              .Where(i => i.Id == testcaseteammember.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(testcaseteammember);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterTestcaseTeammemberUpdated(testcaseteammember);

            return testcaseteammember;
        }

        partial void OnTestcaseTeammemberDeleted(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberDeleted(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

         public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> DeleteTestcaseTeammember(Guid id)
        {
            var itemToDelete = Context.TestcaseTeammembers
                              .Where(i => i.Id == id)
                              .Include(i => i.Buglists)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnTestcaseTeammemberDeleted(itemToDelete);


            Context.TestcaseTeammembers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }


            OnAfterTestcaseTeammemberDeleted(itemToDelete);

            return itemToDelete;
        }
        }
}