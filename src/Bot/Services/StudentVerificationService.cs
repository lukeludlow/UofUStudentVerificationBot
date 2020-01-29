using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace UofUStudentVerificationBot
{
    public class StudentVerificationService
    {
        private readonly IStudentRepository studentRepository;
        private readonly IEmailService emailService;
        private readonly IRoleAssignmentService roleAssignmentService;
        private readonly Random randomGenerator;

        public StudentVerificationService(IServiceProvider serviceProvider)
        {
            this.studentRepository = serviceProvider.GetRequiredService<IStudentRepository>();
            this.emailService = serviceProvider.GetRequiredService<IEmailService>();
            this.roleAssignmentService = serviceProvider.GetRequiredService<IRoleAssignmentService>();
            this.randomGenerator = new Random();
        }

        public VerificationResult BeginVerification(ulong discordID, string uID)
        {
            try {
                VerificationResult validateInputsResult = ValidateBeginVerificationInputs(discordID, uID);
                if (!validateInputsResult.IsSuccess) {
                    return validateInputsResult;
                }
                string verificationCode = GenerateSixDigitVerificationCode();
                Student student = new Student(discordID, uID, verificationCode, false);
                studentRepository.AddOrUpdateStudent(student);
                emailService.SendEmail(uID, verificationCode);
                return VerificationResult.FromSuccess();
            } catch (Exception e) {
                return VerificationResult.FromError(e.Message);
            }
        }

        public VerificationResult CompleteVerification(ulong discordID, string verificationCode)
        {
            try {
                VerificationResult validateInputsResult = ValidateCompleteVerificationInputs(discordID, verificationCode);
                if (!validateInputsResult.IsSuccess) {
                    return validateInputsResult;
                }
                Student student = studentRepository.GetStudentByDiscordID(discordID);
                if (student.VerificationCode == verificationCode) {
                    Student verifiedStudent = new Student(student.DiscordID, student.UID, verificationCode, isVerificationComplete: true);
                    studentRepository.AddOrUpdateStudent(verifiedStudent);
                    roleAssignmentService.AssignVerifiedRoleToDiscordUser(discordID);
                    return VerificationResult.FromSuccess();
                } else {
                    return VerificationResult.FromError("incorrect verification code");
                }
            } catch (Exception e) {
                return VerificationResult.FromError(e.Message);
            }
        }

        public void ResetVerification(ulong discordID)
        {
            roleAssignmentService.RemoveVerifiedRoleFromDiscordUser(discordID);
            studentRepository.RemoveStudentByDiscordID(69);
        }

        public bool IsDiscordUserVerifiedStudent(ulong discordID)
        {
            Student foundStudent = studentRepository.GetStudentByDiscordID(discordID);
            return foundStudent != null && foundStudent.IsVerificationComplete;
        }


        private VerificationResult ValidateBeginVerificationInputs(ulong discordID, string uID)
        {
            if (IsDiscordUserVerifiedStudent(discordID)) {
                return VerificationResult.FromError("user is already verified");
            }
            if (!IsValidUIDFormat(uID)) {
                return VerificationResult.FromError("uID is not properly formatted. your uID should look like this: u1234567");
            }
            if (IsUIDAlreadyVerified(uID)) {
                string errorMessage = "someone else has already verified their student status with that uID.";
                errorMessage += " if you think that this is a mistake, please contact a moderator or my creator (luke).";
                return VerificationResult.FromError(errorMessage);
            }
            return VerificationResult.FromSuccess();
        }

        private VerificationResult ValidateCompleteVerificationInputs(ulong discordID, string verificationCode)
        {
            string uID = studentRepository.GetStudentByDiscordID(discordID).UID;
            if (IsDiscordUserVerifiedStudent(discordID)) {
                return VerificationResult.FromError("user is already verified");
            }
            if (!IsValidVerificationCodeFormat(verificationCode)) {
                return VerificationResult.FromError("verification code is not properly formatted. " +
                    "verification code should be just 6 digits, like this: 123123");
            }
            if (IsUIDAlreadyVerified(uID)) {
                return VerificationResult.FromError("someone else has already verified their student status " +
                    "with that uID. if you think that this is a mistake, please contact a moderator or my creator (luke).");
            }
            return VerificationResult.FromSuccess();
        }


        private bool IsValidUIDFormat(string uID)
        {
            string uIDPattern = @"^u\d{7}$";
            return Regex.IsMatch(uID, uIDPattern);
        }

        private bool IsValidVerificationCodeFormat(string verificationCode)
        {
            string verificationCodePattern = @"^\d{6}$";
            return Regex.IsMatch(verificationCode, verificationCodePattern);
        }

        private bool IsUIDAlreadyVerified(string uID)
        {
            IList<Student> students = studentRepository.GetAllStudents();
            return students.Any(student => student.UID == uID && student.IsVerificationComplete);
        }

        private string GenerateSixDigitVerificationCode()
        {
            return randomGenerator.Next(0, 1000000).ToString("D6");
        }

    }
}