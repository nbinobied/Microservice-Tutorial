using System.ComponentModel.DataAnnotations;

namespace CommandService.Dtos
{
    public class CreateCommandDto
    {
        [Required]
        public string HowTo { get; set; }
        [Required]
        public string CommandLine { get; set; }
    }
}
