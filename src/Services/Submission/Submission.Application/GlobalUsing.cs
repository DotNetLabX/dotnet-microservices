// talk about global using :
// Benefits :
// - reduces boirleplate using statements across the project.
// - makes the files more readable 
// Drawbacks :
// - can lead to confusion about where the types are coming from.
// - can lead to conflicts if the same type name is defined in multiple namespaces (ex: ValidationMessages)

// Third-party libraries
global using MediatR;
global using Mapster;
global using FluentValidation;

// Internal libraries
global using Blocks.Core;
global using Blocks.MediatR;
global using Blocks.EntityFrameworkCore;
global using Blocks.FluentValidation;
global using Articles.Abstractions;
global using Articles.Abstractions.Enums;

// Domain
global using Submission.Domain.Entities;
global using Submission.Domain.Enums;
global using Submission.Domain.ValueObjects;
global using Submission.Domain.StateMachines;

// Application
global using Submission.Application.Features.Shared;

//Persistence
global using Submission.Persistence;
global using Submission.Persistence.Repositories;

global using CachedAssetRepo = Blocks.EntityFrameworkCore.CachedRepository<
				Submission.Persistence.SubmissionDbContext,
				Submission.Domain.Entities.AssetTypeDefinition,
				Articles.Abstractions.Enums.AssetType>;
