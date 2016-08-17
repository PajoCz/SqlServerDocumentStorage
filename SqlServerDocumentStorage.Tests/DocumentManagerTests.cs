using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerDocumentStorage.Tests.Fixtures;

namespace SqlServerDocumentStorage.Tests
{
    [TestClass]
    public class DocumentManagerTests
    {
        [TestMethod]
        public async Task Insert()
        {
            using (var test = await SystemTest.CreateAsync())
            {
                var documentManager = test.GetDocumentManager();

                var space = new Space
                {
                    Name = "trendsales-api",
                    Description = "Trendsales api at https://api.trendsales.com"
                };

                documentManager.Add(space);

                await documentManager.SaveChangesAsync();
                space = await documentManager.GetAsync<Space>(space.Id);
                Assert.AreEqual("trendsales-api", space.Name);
                Assert.AreEqual("Trendsales api at https://api.trendsales.com", space.Description);

                var project = new Project {Title = "Trendsales"};
                documentManager.Add(project);

                await documentManager.SaveChangesAsync();

                var projects =
                    await documentManager.FindWhereAsync<Project>("Title = @Title", new {Title = "Trendsales"});
                Assert.AreEqual(1, projects.Count);
            }
        }

        [TestMethod]
        public async Task CanDelete()
        {
            using (var test = await SystemTest.CreateAsync())
            {
                var documentManager = test.GetDocumentManager();

                var space = new Space
                {
                    Name = "trendsales-api",
                    Description = "Trendsales api at https://api.trendsales.com"
                };

                documentManager.Add(space);

                await documentManager.SaveChangesAsync();
                space = await documentManager.GetAsync<Space>(space.Id);
                Assert.AreEqual("trendsales-api", space.Name);
                Assert.AreEqual("Trendsales api at https://api.trendsales.com", space.Description);

                var project = new Project {Title = "Trendsales"};
                documentManager.Add(project);

                await documentManager.SaveChangesAsync();


                await documentManager.DeleteWhereAsync<Project>("Title = @Title", new {Title = "Trendsales"});

                var projects =
                    await documentManager.FindWhereAsync<Project>("Title = @Title", new {Title = "Trendsales"});
                Assert.AreEqual(0, projects.Count);
            }
        }

        [TestMethod]
        public async Task CanUpdate()
        {
            using (var test = await SystemTest.CreateAsync())
            {
                var documentManager = test.GetDocumentManager();

                var space = new Space
                {
                    Name = "trendsales-api",
                    Description = "Trendsales api at https://api.trendsales.com"
                };

                documentManager.Add(space);

                await documentManager.SaveChangesAsync();
                space = await documentManager.GetAsync<Space>(space.Id);
                Assert.AreEqual("trendsales-api", space.Name);
                Assert.AreEqual("Trendsales api at https://api.trendsales.com", space.Description);

                var project = new Project {Title = "Trendsales"};
                documentManager.Add(project);

                await documentManager.SaveChangesAsync();


                project.Title = "Awesome";

                documentManager.Update(project.Id, project);

                await documentManager.SaveChangesAsync();

                var projects =
                    await documentManager.FindWhereAsync<Project>("Title = @Title", new {Title = "Awesome"});
                Assert.AreEqual(1, projects.Count);
            }
        }
    }
}