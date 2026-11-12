using DalilAcadime.Core.DTOs;
using FluentValidation;

namespace DalilAcadime.Core.Validetors
{
    public class CreateCourseDtoValidators : AbstractValidator<CreateCourseDto>
    {
        public CreateCourseDtoValidators()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(x => x.Level).IsInEnum().WithMessage("Level not valid");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
            RuleFor(x => x.InstructorId).GreaterThan(0).WithMessage("InstructorId is required");
        }
    }

    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(x => x.Level).IsInEnum().WithMessage("Level not valid");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
            RuleFor(x => x.InstructorId).GreaterThan(0).WithMessage("InstructorId is required");
        }
    }
}
