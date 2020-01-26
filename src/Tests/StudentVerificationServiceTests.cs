using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UofUStudentVerificationBot;

namespace UofUStudentVerificationBotTests
{
    [TestClass]
    public class StudentVerificationServiceTests
    {

        private IServiceProvider GetServiceProviderOfMocks()
        {
            return new ServiceCollection()
                .AddSingleton(Mock.Of<IStudentRepository>())
                .AddSingleton(Mock.Of<IEmailService>())
                .AddSingleton(Mock.Of<IRoleAssignmentService>())
                .BuildServiceProvider();
        }


        [TestMethod]
        public void BeginVerification_StudentIsNotVerified_ShouldWritePendingStudentToDatabase()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(value: null);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents()).Returns(new List<Student>() { });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(69, "u1234567");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.GetStudentByDiscordID(69), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student =>
                    student.DiscordID == 69 &&
                    student.UID == "u1234567" &&
                    student.VerificationCode.Length == 6 &&
                    student.IsVerificationComplete == false)), Times.Once());
        }

        [TestMethod]
        public void BeginVerification_StudentIsNotVerified_ShouldSendEmailToStudentWithVerificationCode()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(value: null);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents()).Returns(new List<Student>() { });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(69, "u1234567");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .Verify(m => m.SendEmail("u1234567", It.Is<string>(s => s.Length == 6)), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .VerifyNoOtherCalls();
        }

        [TestMethod]
        public void BeginVerification_StudentHasBegunVerificationProcessButIsIncomplete_ShouldChangeToVerifyWithLatestUID()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(student);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { student });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(69, "u9999999");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.GetStudentByDiscordID(69), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student =>
                    student.DiscordID == 69 &&
                    student.UID == "u9999999" &&
                    student.VerificationCode.Length == 6 &&
                    student.IsVerificationComplete == false)), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .Verify(m => m.SendEmail("u9999999", It.Is<string>(s => s.Length == 6)), Times.Once());
        }

        [TestMethod]
        public void BeginVerification_StudentIsAlreadyVerified_ShouldReturnErrorResult()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: true);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(student);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(69, "u1234567");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.GetStudentByDiscordID(69), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .VerifyNoOtherCalls();
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .VerifyNoOtherCalls();
        }

        [TestMethod]
        public void BeginVerification_SomeoneIsAlreadyVerifiedWithThatUID_ShouldReturnErrorAndNotWriteToDatabase()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { new Student(69, "u1234567", "696969", isVerificationComplete: true) });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(420, "u1234567");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.IsAny<Student>()), Times.Never());
        }

        [TestMethod]
        public void BeginVerification_SomeoneElseIsPendingVerificationWithThatUID_ShouldStillSucceedAndWriteToDatabaseAndSendEmail()
        {
            // two people trying to verify with the same uID at the same time, and that's okay.
            // however, once someone successfully verifies themselves that uID is "locked".
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { new Student(69, "u1234567", "696969", isVerificationComplete: false) });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.BeginVerification(420, "u1234567");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student => student.DiscordID == 420)), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .Verify(m => m.SendEmail("u1234567", It.Is<string>(s => s != "696969")), Times.Once());
        }



        [TestMethod]
        public void CompleteVerification_CorrectVerificationCode_ShouldUpdateDatabaseAndAssignRoleAndReturnTrue()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(student);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { student });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.CompleteVerification(69, "696969");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.GetStudentByDiscordID(69), Times.AtLeastOnce());
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student =>
                    student.DiscordID == 69 &&
                    student.UID == "u1234567" &&
                    student.VerificationCode == "696969" &&
                    student.IsVerificationComplete == true)), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .Verify(m => m.AssignVerifiedRoleToDiscordUser(69), Times.Once());
        }

        [TestMethod]
        public void CompleteVerification_MalformedVerificationCode_ShouldReturnErrorResultAndMakeNoChanges()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(student);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.CompleteVerification(69, "123");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.IsAny<Student>()), Times.Never());
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .VerifyNoOtherCalls();
        }


        [TestMethod]
        public void CompleteVerification_IncorrectVerificationCode_ShouldReturnErrorResultAndMakeNoChanges()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(student);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { student });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.CompleteVerification(69, "000000");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.IsAny<Student>()), Times.Never());
        }

        [TestMethod]
        public void CompleteVerification_StudentIsAlreadyVerified_ShouldReturnErrorResultAndMakeNoChanges()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(new Student(69, "u1234567", "696969", isVerificationComplete: true));
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            // note: it doesn't matter if the verification code is malformed/incorrect, because the user is already verified
            VerificationResult result = verificationService.CompleteVerification(69, "0");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.GetStudentByDiscordID(69), Times.AtLeastOnce());
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .VerifyNoOtherCalls();
            Mock.Get(serviceProvider.GetRequiredService<IEmailService>())
                .VerifyNoOtherCalls();
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .VerifyNoOtherCalls();
        }

        [TestMethod]
        public void CompleteVerification_SomeoneIsAlreadyVerifiedWithThatUID_ShouldReturnErrorAndNotChangeDatabase()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student alreadyVerifiedStudent = new Student(69, "u1234567", "696969", isVerificationComplete: true);
            Student pendingVerificationStudent = new Student(420, "u1234567", "123123", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(alreadyVerifiedStudent);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(420))
                .Returns(pendingVerificationStudent);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { alreadyVerifiedStudent, pendingVerificationStudent });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.CompleteVerification(420, "123123");
            Assert.IsFalse(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.IsAny<Student>()), Times.Never());
        }

        // this means that whoever just sent the verification code "wins"
        [TestMethod]
        public void CompleteVerification_SomeoneElseIsPendingVerificationWithThatUID_ShouldSucceedAndUpdateDatabase()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student pendingVerificationStudent69 = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Student pendingVerificationStudent420 = new Student(420, "u1234567", "123123", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(pendingVerificationStudent69);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(420))
                .Returns(pendingVerificationStudent420);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetAllStudents())
                .Returns(new List<Student>() { pendingVerificationStudent69, pendingVerificationStudent420 });
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            VerificationResult result = verificationService.CompleteVerification(420, "123123");
            Assert.IsTrue(result.IsSuccess);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student =>
                    student.DiscordID == 420 &&
                    student.UID == "u1234567" &&
                    student.IsVerificationComplete == true)), Times.Once());
            // the other person still pending verification shouldn't be changed
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.AddOrUpdateStudent(It.Is<Student>(student => student.DiscordID == 69)), Times.Never());
        }



        [TestMethod]
        public void ResetVerification_StudentIsVerified_ShouldRemoveFromDatabaseAndRemoveRole()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(new Student(69, "u1234567", "696969", isVerificationComplete: true));
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            verificationService.ResetVerification(69);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.RemoveStudentByDiscordID(69), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .Verify(m => m.RemoveVerifiedRoleFromDiscordUser(69), Times.Once());
        }

        [TestMethod]
        public void ResetVerification_StudentBeganVerificationProcessButIsntComplete_ShouldStillRemoveFromDatabaseAndRemoveRole()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(new Student(69, "u1234567", "696969", isVerificationComplete: false));
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            verificationService.ResetVerification(69);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Verify(m => m.RemoveStudentByDiscordID(69), Times.Once());
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .Verify(m => m.RemoveVerifiedRoleFromDiscordUser(69), Times.Once());
        }

        [TestMethod]
        public void ResetVerification_StudentDoesntExistInDatabase_ShouldStillRemoveRole()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69))
                .Returns(value: null);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            verificationService.ResetVerification(69);
            Mock.Get(serviceProvider.GetRequiredService<IRoleAssignmentService>())
                .Verify(m => m.RemoveVerifiedRoleFromDiscordUser(69), Times.Once());
        }



        [TestMethod]
        public void IsDiscordUserVerifiedStudent_NoDbEntry_ShouldReturnFalse()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(value: null);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            bool isVerified = verificationService.IsDiscordUserVerifiedStudent(69);
            Assert.AreEqual(false, isVerified);
        }

        [TestMethod]
        public void IsDiscordUserVerifiedStudent_StudentExistsButVerificationIsNotComplete_ShouldReturnFalse()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: false);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(student);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            bool isVerified = verificationService.IsDiscordUserVerifiedStudent(69);
            Assert.AreEqual(false, isVerified);
        }

        [TestMethod]
        public void IsDiscordUserVerifiedStudent_VerificationIsComplete_ShouldReturnTrue()
        {
            IServiceProvider serviceProvider = GetServiceProviderOfMocks();
            Student student = new Student(69, "u1234567", "696969", isVerificationComplete: true);
            Mock.Get(serviceProvider.GetRequiredService<IStudentRepository>())
                .Setup(m => m.GetStudentByDiscordID(69)).Returns(student);
            StudentVerificationService verificationService = new StudentVerificationService(serviceProvider);
            bool isVerified = verificationService.IsDiscordUserVerifiedStudent(69);
            Assert.AreEqual(true, isVerified);
        }

    }
}
