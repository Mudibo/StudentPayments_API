namespace StudentPayments_API.Services.Interfaces;

//The interface below defines a contract for student validation, returning a tuple if the student is valid, the student record if found, and a message
public interface IStudentValidationService
//Define an interface for student validation services
{
    //Declare a method for validating a student asynchronously
    //  -Takes admission number, mobile number, program as input
    //  -Returns a task for async operation
    Task<(bool isValid, Models.Student? student, string message)> ValidateStudentAsync(string admissionNumber, string program, string mobileNumber);
    //Tuple return type: isValid (if a student is valid), student (the student object if found)
    //Models.Student? indicates student can be null if not found
}