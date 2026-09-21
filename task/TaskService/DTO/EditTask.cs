using System;
using System.ComponentModel.DataAnnotations;
using TaskService.DTO;

namespace TaskService;

public record EditTask
(
    [StringLength(150)]
    string Title,
    [StringLength(1000)]
    string Description
);
