using System.ComponentModel.DataAnnotations;

namespace TaskService.DTO;

public record EditTask
(
    [StringLength(150)]
    string Title,
    [StringLength(1000)]
    string Description
);
