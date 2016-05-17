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
                
                Space space = new Space();
                space.Name = "trendsales-api";
                space.Description = "Trendsales api at https://api.trendsales.com";

                documentManager.Add(space);
                
                await documentManager.SaveChangesAsync();
                space = await documentManager.GetAsync<Space>(space.Id);
                Assert.AreEqual("trendsales-api", space.Name);
                Assert.AreEqual("Trendsales api at https://api.trendsales.com", space.Description);

                var project = new Project();
                project.Title = "Trendsales";
                documentManager.Add(project);

                await documentManager.SaveChangesAsync();

                var projects = await documentManager.FindWhereAsync<Project>("Title = @Title", new {Title = "Trendsales"});
                Assert.AreEqual(1, projects.Count);


            }
        }
    }
}