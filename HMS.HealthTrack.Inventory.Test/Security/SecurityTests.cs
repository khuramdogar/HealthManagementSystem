using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Security
{
	[TestClass]
	public class SecurityTests
	{
		[TestMethod]
		public void TestUserWithNoGroups()
		{
			//Arrange
			var mockedId = new Mock<IIdentity>();
			mockedId.Setup(m => m.Name).Returns("Bob");
			
			//Test groups
			var groups = new List<HealthTrackGroup>();
			
			//Act
			var selectedGroups = GroupUtils.GetGroupsForUser(mockedId.Object.Name, groups.AsQueryable());

			//Assert
         Assert.AreEqual(0,selectedGroups.Count);
		}

		[TestMethod]
		public void TestAUserInsideAGroup()
		{
			//Arrange
			var mockedId = new Mock<IIdentity>();
			mockedId.Setup(m => m.Name).Returns("Bob");

			//Test groups
			var groups = new List<HealthTrackGroup> {new HealthTrackGroup {base_ID = "Administrators", member_ID = "Bob"}};

			//Act
			var selectedGroups = GroupUtils.GetGroupsForUser(mockedId.Object.Name, groups.AsQueryable());

			//Assert
			Assert.AreEqual(1, selectedGroups.Count);
			Assert.AreEqual("administrators", selectedGroups.First());
		}

		[TestMethod]
		public void TestGroupsInsideGroups()
		{
			//Arrange
			var mockedId = new Mock<IIdentity>();
			mockedId.Setup(m => m.Name).Returns("Bob");

			//Test groups belonging to bob
			var groups = new List<HealthTrackGroup>
			{
				new HealthTrackGroup {base_ID = "Receptionists", member_ID = "Bob"},
				new HealthTrackGroup {base_ID = "Administrators", member_ID = "Receptionists"}
			};

			//Test groups not belonging to bob
			groups.Add(new HealthTrackGroup { base_ID = "herder", member_ID = "Jeff" });

			//Act
			var selectedGroups = GroupUtils.GetGroupsForUser(mockedId.Object.Name, groups.AsQueryable());

			//Assert
			Assert.AreEqual(2, selectedGroups.Count);
			Assert.IsTrue(selectedGroups.Contains("administrators"));
		}
	}
}
