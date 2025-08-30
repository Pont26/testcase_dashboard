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
          private readonly IDbContextFactory<mydatabaseContext> _contextFactory;
    private readonly NavigationManager navigationManager;

    // Use a private property to create a fresh context for each operation
    private mydatabaseContext Context => _contextFactory.CreateDbContext();

    // The single, corrected constructor
    public mydatabaseService(IDbContextFactory<mydatabaseContext> contextFactory, NavigationManager navigationManager)
    {
        _contextFactory = contextFactory;
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


       
        partial void OnBuglistsRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Buglist> items);

    public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.Buglist>> GetBuglists(Query query = null)
{
    // The key change: create a NEW, isolated context for this one operation
    // from the factory.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.Buglists.AsQueryable();

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

        // Crucial fix: Execute the query asynchronously and return the result.
        // This materializes the data BEFORE the `using` block ends.
        var result = await items.ToListAsync();

        return result.AsQueryable();
    } // The context is automatically disposed of here
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
    // Create a new, isolated context for this single operation
    using (var context = _contextFactory.CreateDbContext())
    {
        OnBuglistCreated(buglist);

        // Check for an existing item on the same context
        var existingItem = await context.Buglists
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(i => i.Id == buglist.Id);
        
        if (existingItem != null)
        {
            throw new Exception("Item already available");
        }

        try
        {
            // Add the new buglist item to the fresh context.
            context.Buglists.Add(buglist);

            // Set the creation and update timestamps.
            var now = DateTime.UtcNow;
            buglist.Createdat = now;
            buglist.Updatedat = now;

            // Save the changes to the database.
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // The `using` block will automatically dispose the context,
            // so manually detaching the entity is no longer necessary.
            throw;
        }

        OnAfterBuglistCreated(buglist);

        return buglist;
    }
}
      public async Task<TestCaseDashboard.Models.mydatabase.Buglist> CancelBuglistChanges(TestCaseDashboard.Models.mydatabase.Buglist item)
{
    // Use the correct name: `_contextFactory`.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Fetch the original item asynchronously.
        var originalItem = await context.Buglists
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(i => i.Id == item.Id);
        
        // Return the original item.
        return originalItem;
    }
}
        partial void OnBuglistUpdated(TestCaseDashboard.Models.mydatabase.Buglist item);
        partial void OnAfterBuglistUpdated(TestCaseDashboard.Models.mydatabase.Buglist item);

        public async Task<TestCaseDashboard.Models.mydatabase.Buglist> UpdateBuglist(Guid id, TestCaseDashboard.Models.mydatabase.Buglist buglist)
{
    // 💡 Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnBuglistUpdated(buglist);

        // Find the existing item asynchronously using the new context.
        var itemToUpdate = await context.Buglists
                                        .Where(i => i.Id == buglist.Id)
                                        .FirstOrDefaultAsync(); // 💡 Use async method

        if (itemToUpdate == null)
        {
            throw new Exception("Item no longer available");
        }
        
        // Update properties manually.
        itemToUpdate.Remark = buglist.Remark;
        itemToUpdate.Image = buglist.Image;
        itemToUpdate.Updatedat = DateTime.UtcNow;

        // Save the changes to the database.
        await context.SaveChangesAsync();

        return itemToUpdate;
    }
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
    
       
        partial void OnProjectsRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Project> items);

       public async Task<IEnumerable<TestCaseDashboard.Models.mydatabase.Project>> GetProjects(Query query = null)
{
    // Use a single, short-lived DbContext for this operation.
    using var dbContext = _contextFactory.CreateDbContext();

    var items = dbContext.Projects.AsQueryable();

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

    OnProjectsRead(ref items);

    // 💡 Fix: Execute the query asynchronously and return the list.
    // This materializes the data BEFORE the 'using' block ends.
    return await items.ToListAsync();
}

        partial void OnProjectGet(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnGetProjectById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Project> items);


        public async Task<TestCaseDashboard.Models.mydatabase.Project> GetProjectById(Guid id)
{
    // Use a single DbContext for the entire method
    using var dbContext = _contextFactory.CreateDbContext();

    // Use a single variable for the query
    var items = dbContext.Projects
                         .AsNoTracking()
                         .Where(i => i.Id == id);
    
    OnGetProjectById(ref items);

    // Asynchronously get the first item
    var itemToReturn = await items.FirstOrDefaultAsync();

    OnProjectGet(itemToReturn);

    return itemToReturn;
}

        partial void OnProjectCreated(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnAfterProjectCreated(TestCaseDashboard.Models.mydatabase.Project item);

       public async Task<TestCaseDashboard.Models.mydatabase.Project> CreateProject(TestCaseDashboard.Models.mydatabase.Project project)
{
    // Use a using block to ensure a single context for all operations and proper disposal
    using var dbContext = _contextFactory.CreateDbContext();

    // The 'OnProjectCreated' method is a partial method, so it's a good practice to include it here
    OnProjectCreated(project);

    // Check for an existing item on the same context
    var existingItem = await dbContext.Projects.FirstOrDefaultAsync(i => i.Id == project.Id);
    if (existingItem != null)
    {
        throw new Exception("Item already available");
    }

    try
    {
        // Automatically set the creation and update timestamps
        var now = DateTime.UtcNow;
        project.Createdat = now;
        project.Updatedat = now;

        // Add the new project to the same context
        dbContext.Projects.Add(project);

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }
    catch (Exception)
    {
        // Detaching the entity is no longer necessary here because the 'using' block
        // will dispose of the context and its tracked entities automatically.
        throw;
    }

    // The 'OnAfterProjectCreated' method is a partial method
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
    // 1. Create a new DbContext instance for this operation.
    // The 'Context' property already does this, which is good.
    var dbContext = Context; 
    
    // 2. Find the existing item.
    var itemToUpdate = await dbContext.Projects
        .FirstOrDefaultAsync(i => i.Id == id);

    if (itemToUpdate == null)
    {
        throw new Exception("Item no longer available");
    }

    // 3. Update the tracked entity with the new values.
    itemToUpdate.Projectname = project.Projectname;
    itemToUpdate.ProjectSource = project.ProjectSource;
    itemToUpdate.Updatedat = DateTime.UtcNow;

    // 4. Save the changes. The context is now tracking 'itemToUpdate' because it was retrieved
    // by a query on that same context instance, so SaveChangesAsync will work.
    await dbContext.SaveChangesAsync();

    return itemToUpdate;
}
        partial void OnProjectDeleted(TestCaseDashboard.Models.mydatabase.Project item);
        partial void OnAfterProjectDeleted(TestCaseDashboard.Models.mydatabase.Project item);

      public async Task<TestCaseDashboard.Models.mydatabase.Project> DeleteProject(Guid id)
{
    using (var context = _contextFactory.CreateDbContext())
    {
        // Fetch the project and its related children. This is the correct step.
        var itemToDelete = await context.Projects
                                        .Include(i => i.ProjectTeammembers)
                                        .Include(i => i.Testcases)
                                        .FirstOrDefaultAsync(i => i.Id == id);

        if (itemToDelete == null)
        {
            throw new ApplicationException("Item no longer available");
        }

        OnProjectDeleted(itemToDelete);

        // 💡 The Fix: Manually delete the related "child" records first.
        if (itemToDelete.ProjectTeammembers != null)
        {
            context.ProjectTeammembers.RemoveRange(itemToDelete.ProjectTeammembers);
        }

        if (itemToDelete.Testcases != null)
        {
            context.Testcases.RemoveRange(itemToDelete.Testcases);
        }

        // Now, delete the parent record.
        context.Projects.Remove(itemToDelete);

        try
        {
            // Save all changes in a single transaction.
            await context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }

        OnAfterProjectDeleted(itemToDelete);

        return itemToDelete;
    }
}
      
        partial void OnProjectTeammembersRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember> items);

       public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember>> GetProjectTeammembers(Query query = null)
{
    // 💡 The Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Include Project and Teammember
        var items = context.ProjectTeammembers
                            .Include(i => i.Project)
                            .Include(i => i.Teammember)
                            .AsQueryable();

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

            // Apply any filters from the query object
            ApplyQuery(ref items, query);
        }

        OnProjectTeammembersRead(ref items);

        // 💡 The Fix: Execute the query asynchronously and return the result.
        // This materializes the query into a list.
        var result = await items.ToListAsync();
        
        // Return the list as an IQueryable.
        return result.AsQueryable();
    }
}

        partial void OnProjectTeammemberGet(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnGetProjectTeammemberById(ref IQueryable<TestCaseDashboard.Models.mydatabase.ProjectTeammember> items);


       public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> GetProjectTeammemberById(Guid id)
{
    // Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.ProjectTeammembers
                            .AsNoTracking()
                            .Where(i => i.Id == id);

        items = items.Include(i => i.Project);
        items = items.Include(i => i.Teammember);

        OnGetProjectTeammemberById(ref items);

        // Perform the query asynchronously.
        var itemToReturn = await items.FirstOrDefaultAsync();

        OnProjectTeammemberGet(itemToReturn);

        // Return the result directly without wrapping it in a task.
        return itemToReturn;
    }
}

        partial void OnProjectTeammemberCreated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberCreated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

       public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> CreateProjectTeammember(TestCaseDashboard.Models.mydatabase.ProjectTeammember projectteammember)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnProjectTeammemberCreated(projectteammember);

        // Check for an existing item asynchronously.
        var existingItem = await context.ProjectTeammembers
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(i => i.Id == projectteammember.Id);

        if (existingItem != null)
        {
            throw new Exception("Item already available");
        }

        try
        {
            context.ProjectTeammembers.Add(projectteammember);
            // Use the asynchronous method to save changes.
            await context.SaveChangesAsync();
        }
        catch
        {
            // Detaching the entity is not necessary here because the context will be disposed.
            throw;
        }

        OnAfterProjectTeammemberCreated(projectteammember);

        return projectteammember;
    }
}

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> CancelProjectTeammemberChanges(TestCaseDashboard.Models.mydatabase.ProjectTeammember item)
{
    // With the DbContextFactory pattern, you don't need to manually
    // manage state. The item passed to this method is a detached entity.
    // To "cancel" changes, we simply fetch the original item from the database.

    using (var context = _contextFactory.CreateDbContext())
    {
        // 💡 Fetch the original item asynchronously.
        var originalItem = await context.ProjectTeammembers
                                       .AsNoTracking() // Use AsNoTracking for read-only query.
                                       .FirstOrDefaultAsync(i => i.Id == item.Id);
        
        // Return the original item.
        return originalItem;
    }
}
        partial void OnProjectTeammemberUpdated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberUpdated(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

       public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> UpdateProjectTeammember(Guid id, TestCaseDashboard.Models.mydatabase.ProjectTeammember projectteammember)
{
    // 💡 Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnProjectTeammemberUpdated(projectteammember);

        var itemToUpdate = await context.ProjectTeammembers
                                        .Where(i => i.Id == id)
                                        .FirstOrDefaultAsync();

        if (itemToUpdate == null)
        {
            throw new ApplicationException("Item no longer available");
        }
            
        try
        {
            // 💡 Use SetValues to efficiently update all properties.
            context.Entry(itemToUpdate).CurrentValues.SetValues(projectteammember);

            await context.SaveChangesAsync(); // 💡 Use async method.
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // 💡 Handle concurrency conflicts.
            if (!await context.ProjectTeammembers.AnyAsync(t => t.Id == id))
            {
                throw new ApplicationException("Item was deleted by another user.");
            }
            throw; // Rethrow the exception for the UI to handle.
        }

        OnAfterProjectTeammemberUpdated(projectteammember);

        return itemToUpdate;
    }
}
        partial void OnProjectTeammemberDeleted(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);
        partial void OnAfterProjectTeammemberDeleted(TestCaseDashboard.Models.mydatabase.ProjectTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.ProjectTeammember> DeleteProjectTeammember(Guid id)
{
    // Create a new, temporary context for this single operation
    using (var context = _contextFactory.CreateDbContext())
    {
        var itemToDelete = context.ProjectTeammembers // use 'context' instead of 'Context'
                                 .Where(i => i.Id == id)
                                 .FirstOrDefault();

        if (itemToDelete == null)
        {
            throw new Exception("Item no longer available");
        }

        OnProjectTeammemberDeleted(itemToDelete);

        context.ProjectTeammembers.Remove(itemToDelete); // use 'context'

        try
        {
            context.SaveChanges(); // use 'context'
        }
        catch
        {
            context.Entry(itemToDelete).State = EntityState.Unchanged; // use 'context'
            throw;
        }

        OnAfterProjectTeammemberDeleted(itemToDelete);

        return itemToDelete;
    } // The context is disposed of automatically here
}
    
    

        partial void OnTeammembersRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Teammember> items);
public async Task<IEnumerable<TestCaseDashboard.Models.mydatabase.Teammember>> GetTeammembers(Query query = null)
{
    // 💡 The fix: Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.Teammembers.AsQueryable();

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
}

        partial void OnTeammemberGet(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnGetTeammemberById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Teammember> items);


       public async Task<TestCaseDashboard.Models.mydatabase.Teammember> GetTeammemberById(Guid id)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.Teammembers
                            .AsNoTracking()
                            .Where(i => i.Id == id);
        
        // You might need to adjust this if OnGetTeammemberById is a synchronous method.
        OnGetTeammemberById(ref items);

        // Perform the query asynchronously.
        var itemToReturn = await items.FirstOrDefaultAsync();

        OnTeammemberGet(itemToReturn);

        // Return the result directly.
        return itemToReturn;
    }
}
        partial void OnTeammemberCreated(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnAfterTeammemberCreated(TestCaseDashboard.Models.mydatabase.Teammember item);

       public async Task<TestCaseDashboard.Models.mydatabase.Teammember> CreateTeammember(
    TestCaseDashboard.Models.mydatabase.Teammember teammember)
{
    // 💡 The Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnTeammemberCreated(teammember);

        // Check for an existing item with the same ID.
        var existingItem = await context.Teammembers
                                         .AsNoTracking() // Use AsNoTracking for read-only queries.
                                         .FirstOrDefaultAsync(i => i.Id == teammember.Id);

        if (existingItem != null)
        {
            throw new Exception("Item already available");
        }

        try
        {
            var now = DateTime.UtcNow;
            teammember.Createdat = now;
            teammember.Updatedat = now;

            context.Teammembers.Add(teammember);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Detaching the entity is no longer strictly necessary because the context is disposed.
            // But it's good practice if you want to reuse the entity later.
            // context.Entry(teammember).State = EntityState.Detached; 
            throw;
        }

        OnAfterTeammemberCreated(teammember);

        return teammember;
    }
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
    // 💡 Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Find the existing item by ID
        var itemToUpdate = await context.Teammembers
                                       .FirstOrDefaultAsync(i => i.Id == id);

        if (itemToUpdate == null)
        {
            // 💡 Better Exception: Use a more specific, descriptive exception.
            throw new ApplicationException("Item no longer available.");
        }

        try
        {
            // 💡 Fix: Use SetValues to efficiently update all properties.
            context.Entry(itemToUpdate).CurrentValues.SetValues(teammember);

            // 💡 Fix: Explicitly update Updatedat.
            itemToUpdate.Updatedat = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // 💡 Fix: Catch the specific concurrency exception.
            if (!await context.Teammembers.AnyAsync(t => t.Id == id))
            {
                // Item was deleted by another user.
                throw new ApplicationException("Item was deleted by another user.");
            }
            throw; // Rethrow to let the UI handle the conflict.
        }

        return itemToUpdate;
    }
}

        partial void OnTeammemberDeleted(TestCaseDashboard.Models.mydatabase.Teammember item);
        partial void OnAfterTeammemberDeleted(TestCaseDashboard.Models.mydatabase.Teammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.Teammember> DeleteTeammember(Guid id)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Fetch the item asynchronously, including related data.
        var itemToDelete = await context.Teammembers
                                       .Where(i => i.Id == id)
                                       .Include(i => i.ProjectTeammembers)
                                       .Include(i => i.TestcaseTeammembers)
                                       .FirstOrDefaultAsync(); // 💡 Use async method

        if (itemToDelete == null)
        {
            throw new ApplicationException("Item no longer available");
        }

        OnTeammemberDeleted(itemToDelete);

        context.Teammembers.Remove(itemToDelete);

        try
        {
            await context.SaveChangesAsync(); // 💡 Use async method
        }
        catch (Exception)
        {
            // Detach the entity to prevent it from being tracked
            context.Entry(itemToDelete).State = EntityState.Detached;
            throw;
        }

        OnAfterTeammemberDeleted(itemToDelete);

        return itemToDelete;
    }
}
     

        partial void OnTestcasesRead(ref IQueryable<TestCaseDashboard.Models.mydatabase.Testcase> items);

        public async Task<IQueryable<TestCaseDashboard.Models.mydatabase.Testcase>> GetTestcases(Query query = null)
{
    // 💡 The Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Include Project and TestcaseTeammembers with Teammember
        var items = context.Testcases
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

            // Apply any filters from the query object
            ApplyQuery(ref items, query);
        }

        OnTestcasesRead(ref items);

        // 💡 The Fix: Execute the query asynchronously and return the result.
        // This materializes the query, ensuring the DbContext is not disposed prematurely.
        var result = await items.ToListAsync();
        
        return result.AsQueryable();
    }
}

        partial void OnTestcaseGet(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnGetTestcaseById(ref IQueryable<TestCaseDashboard.Models.mydatabase.Testcase> items);


public async Task<TestCaseDashboard.Models.mydatabase.Testcase> GetTestcaseById(Guid id)
{
    // 💡 The Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.Testcases
                            .Include(t => t.TestcaseTeammembers)
                            .ThenInclude(tm => tm.Teammember)
                            .Where(t => t.Id == id);

        // Perform the query asynchronously.
        return await items.FirstOrDefaultAsync();
    }
}


        partial void OnTestcaseCreated(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseCreated(TestCaseDashboard.Models.mydatabase.Testcase item);

      public async Task<TestCaseDashboard.Models.mydatabase.Testcase> CreateTestcase(TestCaseDashboard.Models.mydatabase.Testcase testcase)
{
    // 💡 Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnTestcaseCreated(testcase);

        // 💡 Check for an existing item asynchronously.
        var existingItem = await context.Testcases
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(i => i.Id == testcase.Id);

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
            context.Testcases.Add(testcase);
            // 💡 Use the asynchronous method to save changes.
            await context.SaveChangesAsync();
        }
        catch
        {
            // Detaching the entity is not necessary here because the context will be disposed.
            throw;
        }

        OnAfterTestcaseCreated(testcase);

        return testcase;
    }
}

        partial void OnTestcaseUpdated(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseUpdated(TestCaseDashboard.Models.mydatabase.Testcase item);

   public async Task<TestCaseDashboard.Models.mydatabase.Testcase> UpdateTestcase(Guid id, TestCaseDashboard.Models.mydatabase.Testcase updatedTestcase)
{
    using var db = _contextFactory.CreateDbContext();

    // 1. Load existing testcase with children
    var existingTestcase = await db.Testcases
        .Include(t => t.TestcaseTeammembers)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (existingTestcase == null)
        throw new Exception("Testcase not found");

    // 2. Update scalar properties
    db.Entry(existingTestcase).CurrentValues.SetValues(updatedTestcase);

    // 3. Sync child collection
    var existingMembers = existingTestcase.TestcaseTeammembers.ToList();

    // Remove deleted members
    foreach (var em in existingMembers)
    {
        if (!updatedTestcase.TestcaseTeammembers.Any(utm => utm.Teammemberid == em.Teammemberid && utm.Role == em.Role))
        {
            db.TestcaseTeammembers.Remove(em);
        }
    }

    // Add or update members
    foreach (var tm in updatedTestcase.TestcaseTeammembers)
    {
        var existing = existingMembers.FirstOrDefault(em => em.Teammemberid == tm.Teammemberid && em.Role == tm.Role);
        if (existing != null)
        {
            existing.TestStatus = tm.TestStatus;
        }
        else
        {
            tm.Testcaseid = existingTestcase.Id; // set FK
            db.TestcaseTeammembers.Add(tm);
        }
    }

    await db.SaveChangesAsync();
    return existingTestcase;
}



        partial void OnTestcaseDeleted(TestCaseDashboard.Models.mydatabase.Testcase item);
        partial void OnAfterTestcaseDeleted(TestCaseDashboard.Models.mydatabase.Testcase item);

       public async Task<TestCaseDashboard.Models.mydatabase.Testcase> DeleteTestcase(Guid id)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Fetch the item asynchronously, including related data.
        var itemToDelete = await context.Testcases
                                       .Where(i => i.Id == id)
                                       .Include(i => i.TestcaseTeammembers)
                                       .FirstOrDefaultAsync(); // 💡 Use async method

        if (itemToDelete == null)
        {
            throw new ApplicationException("Item no longer available");
        }

        OnTestcaseDeleted(itemToDelete);

        context.Testcases.Remove(itemToDelete);

        try
        {
            await context.SaveChangesAsync(); // 💡 Use async method
        }
        catch (Exception)
        {
            // Detach the entity to prevent it from being tracked
            context.Entry(itemToDelete).State = EntityState.Detached;
            throw;
        }

        OnAfterTestcaseDeleted(itemToDelete);

        return itemToDelete;
    }
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
    // 💡 Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        var items = context.TestcaseTeammembers
                            .AsNoTracking()
                            .Where(i => i.Id == id);

        items = items.Include(i => i.Teammember);
        items = items.Include(i => i.Testcase);

        OnGetTestcaseTeammemberById(ref items);

        // 💡 Fix: Perform the query asynchronously.
        var itemToReturn = await items.FirstOrDefaultAsync();

        OnTestcaseTeammemberGet(itemToReturn);

        // 💡 Fix: Return the result directly without wrapping it in a task.
        return itemToReturn;
    }
}
        partial void OnTestcaseTeammemberCreated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberCreated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> CreateTestcaseTeammember(TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseteammember)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnTestcaseTeammemberCreated(testcaseteammember);

        // Check for an existing item asynchronously.
        var existingItem = await context.TestcaseTeammembers
                                        .Where(i => i.Id == testcaseteammember.Id)
                                        .FirstOrDefaultAsync();

        if (existingItem != null)
        {
            throw new Exception("Item already available");
        }

        try
        {
            context.TestcaseTeammembers.Add(testcaseteammember);
            // Use the asynchronous method to save changes.
            await context.SaveChangesAsync();
        }
        catch
        {
            // Detaching the entity is not necessary here because the context will be disposed.
            throw;
        }

        OnAfterTestcaseTeammemberCreated(testcaseteammember);

        return testcaseteammember;
    }
}

        public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> CancelTestcaseTeammemberChanges(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item)
{
    // With the DbContextFactory pattern, you don't need to manually
    // manage state. The item passed to this method is a detached entity.
    // If you need to "cancel" changes, you simply fetch the original
    // item from the database.

    // 💡 Fetch the original item from the database based on its ID.
    // This is the most reliable way to get the "canceled" state.
    // Use the DbContextFactory to ensure a fresh context.
    using (var context = _contextFactory.CreateDbContext())
    {
        var originalItem = await context.TestcaseTeammembers.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
        
        // Return the original item.
        return originalItem;
    }
}

        partial void OnTestcaseTeammemberUpdated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberUpdated(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

       public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> UpdateTestcaseTeammember(Guid id, TestCaseDashboard.Models.mydatabase.TestcaseTeammember testcaseteammember)
{
    // 💡 Fix: Use a new, short-lived DbContext instance.
    using (var context = _contextFactory.CreateDbContext())
    {
        OnTestcaseTeammemberUpdated(testcaseteammember);

        var itemToUpdate = await context.TestcaseTeammembers
                                        .Where(i => i.Id == id)
                                        .FirstOrDefaultAsync();

        if (itemToUpdate == null)
        {
            throw new ApplicationException("Item no longer available");
        }
            
        try
        {
            // 💡 Fix: Use SetValues to efficiently update all properties.
            context.Entry(itemToUpdate).CurrentValues.SetValues(testcaseteammember);

            await context.SaveChangesAsync(); // 💡 Fix: Use async method.
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // 💡 Fix: Handle concurrency conflicts.
            if (!await context.TestcaseTeammembers.AnyAsync(t => t.Id == id))
            {
                throw new ApplicationException("Item was deleted by another user.");
            }
            throw; // Rethrow the exception for the UI to handle.
        }

        OnAfterTestcaseTeammemberUpdated(testcaseteammember);

        return itemToUpdate;
    }
}

        partial void OnTestcaseTeammemberDeleted(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);
        partial void OnAfterTestcaseTeammemberDeleted(TestCaseDashboard.Models.mydatabase.TestcaseTeammember item);

         public async Task<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> DeleteTestcaseTeammember(Guid id)
{
    // Use a fresh DbContext instance for this operation.
    using (var context = _contextFactory.CreateDbContext())
    {
        // Fetch the item asynchronously, including related data.
        var itemToDelete = await context.TestcaseTeammembers
                                       .Where(i => i.Id == id)
                                       .Include(i => i.Buglists)
                                       .FirstOrDefaultAsync();

        if (itemToDelete == null)
        {
            throw new ApplicationException("Item no longer available");
        }

        OnTestcaseTeammemberDeleted(itemToDelete);

        context.TestcaseTeammembers.Remove(itemToDelete);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Detach the entity to prevent it from being tracked
            context.Entry(itemToDelete).State = EntityState.Detached;
            throw;
        }

        OnAfterTestcaseTeammemberDeleted(itemToDelete);

        return itemToDelete;
    }
}
        }
}