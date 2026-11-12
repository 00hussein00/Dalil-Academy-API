using DalilAcadime.Core.DTOs;
using FluentValidation;

namespace DalilAcadime.Core.Validetors
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must be less than 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }

    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must be less than 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
