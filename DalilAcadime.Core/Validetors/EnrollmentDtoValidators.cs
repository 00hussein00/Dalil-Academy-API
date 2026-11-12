using DalilAcadime.Core.DTOs;
using FluentValidation;

namespace DalilAcadime.Core.Validetors
{
    public class CreateEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
    {
        public CreateEnrollmentDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("StudentId is required");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("CourseId is required");
        }
    }

    public class UpdateEnrollmentDtoValidator : AbstractValidator<UpdateEnrollmentDto>
    {
        public UpdateEnrollmentDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("StudentId is required");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("CourseId is required");
        }
    }
}
